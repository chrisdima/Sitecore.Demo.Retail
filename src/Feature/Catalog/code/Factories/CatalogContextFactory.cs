﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Sitecore.Commerce.Connect.CommerceServer;
using Sitecore.Commerce.Connect.CommerceServer.Caching;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Feature.Commerce.Catalog.Models;
using Sitecore.Feature.Commerce.Catalog.Services;
using Sitecore.Foundation.Commerce.Managers;
using Sitecore.Foundation.Commerce.Models;
using Sitecore.Foundation.SitecoreExtensions.Extensions;

namespace Sitecore.Feature.Commerce.Catalog.Factories
{
    public class CatalogContextFactory
    {
        public CatalogContextFactory(CatalogManager catalogManager, CatalogUrlService catalogUrlRepository)
        {
            CacheProvider = CommerceTypeLoader.GetCacheProvider(CommerceConstants.KnownCacheNames.FriendlyUrlsCache);
            CatalogManager = catalogManager;
            CatalogUrlRepository = catalogUrlRepository;
        }

        private CatalogManager CatalogManager { get; }
        public CatalogUrlService CatalogUrlRepository { get; }

        private ICacheProvider CacheProvider { get; }

        private void AddCatalogItemToCache(string catalogId, string catalogName, Item foundItem)
        {
            CacheProvider.AddData(CommerceConstants.KnownCachePrefixes.Sitecore, CommerceConstants.KnownCacheNames.FriendlyUrlsCache, GetCachekey(catalogId, catalogName), foundItem != null ? foundItem.ID : ID.Undefined);
        }

        private ID GetCatalogItemFromCache(string catalogId, string catalogName)
        {
            return CacheProvider.GetData<ID>(CommerceConstants.KnownCachePrefixes.Sitecore, CommerceConstants.KnownCacheNames.FriendlyUrlsCache, GetCachekey(catalogId, catalogName));
        }

        private static string GetCachekey(string itemId, string catalogName)
        {
            return "FriendlyUrl-" + itemId + "-" + catalogName;
        }


        public ICatalogContext Create(RouteData routeData, Database database)
        {
            Assert.IsNotNull(database, nameof(database));
            Assert.IsNotNull(routeData, nameof(routeData));

            var itemType = RouteConfig.GetItemType(routeData);
            switch (itemType)
            {
                case RouteItemType.CatalogItem:
                    return CreateCatalogContextFromCatalogRoute(routeData);
                case RouteItemType.Product:
                    return CreateCatalogContextFromProductRoute(routeData, database);
                case RouteItemType.Category:
                    return CreateCatalogContextFromCategoryRoute(routeData, database);
                default:
                    return CreateEmptyCatalogContext();
            }
        }

        private ICatalogContext Create(Item productCatalogItem)
        {
            Assert.IsNotNull(productCatalogItem, nameof(productCatalogItem));
            Assert.ArgumentCondition(productCatalogItem.IsDerived(Foundation.Commerce.Templates.Commerce.CatalogItem.ID), nameof(productCatalogItem), "Item must be of type Commerce Catalog Item");

            var data = new CatalogRouteData
            {
                ItemType = productCatalogItem.IsDerived(Foundation.Commerce.Templates.Commerce.Product.ID) ? CatalogItemType.Product : CatalogItemType.Category,
                Id = productCatalogItem.Name.ToLowerInvariant(),
                Item = productCatalogItem,
                Catalog = productCatalogItem[Foundation.Commerce.Templates.Commerce.CatalogItem.Fields.CatalogName],
                CategoryId = GetCategoryIdFromItem(productCatalogItem)
            };

            return data;
        }

        private ICatalogContext CreateEmptyCatalogContext()
        {
            var data = new CatalogRouteData
            {
                Catalog = CatalogManager.CurrentCatalog?.Name
            };

            return data;
        }

        private ICatalogContext CreateCatalogContextFromProductRoute(RouteData routeData, Database database)
        {
            return CreateCatalogContextFromCatalogItemRoute(routeData, CatalogItemType.Product, database);
        }

        private ICatalogContext CreateCatalogContextFromCategoryRoute(RouteData routeData, Database database)
        {
            return CreateCatalogContextFromCatalogItemRoute(routeData, CatalogItemType.Category, database);
        }

        private ICatalogContext CreateCatalogContextFromCatalogItemRoute(RouteData routeData, CatalogItemType itemType, Database database)
        {
            var data = new CatalogRouteData
            {
                ItemType = itemType
            };

            if (routeData.Values.ContainsKey("id"))
            {
                data.Id = CatalogUrlRepository.ExtractItemId(routeData.Values["id"].ToString());
            }

            if (string.IsNullOrWhiteSpace(data.Id))
            {
                return null;
            }

            if (routeData.Values.ContainsKey("catalog"))
            {
                data.Catalog = routeData.Values["catalog"].ToString();
            }
            if (string.IsNullOrEmpty(data.Catalog))
            {
                data.Catalog = CatalogManager.CurrentCatalog?.Name;
            }
            if (string.IsNullOrEmpty(data.Catalog))
            {
                return null;
            }

            var item = GetCatalogItem(data, database);
            if (item == null)
                return null;
            data.Item = item;

            if (routeData.Values.ContainsKey("category"))
            {
                data.CategoryId = CatalogUrlRepository.ExtractItemId(routeData.Values["category"].ToString());
            }
            if (string.IsNullOrEmpty(data.CategoryId))
            {
                data.CategoryId = GetCategoryIdFromItem(data.Item);
            }

            return data;
        }

        private Item GetCatalogItem(ICatalogContext data, Database database)
        {
            var id = GetCatalogItemFromCache(data.Id, data.Catalog);
            if (!ID.IsNullOrEmpty(id))
            {
                return id == ID.Undefined ? null : database.GetItem(id);
            }

            Item item = null;
            switch (data.ItemType)
            {
                case CatalogItemType.Product:
                    item = CatalogManager.GetProduct(data.Id, data.Catalog);
                    break;
                case CatalogItemType.Category:
                    item = CatalogManager.GetCategoryItem(data.Id, data.Catalog);
                    break;
            }
            if (item != null)
                AddCatalogItemToCache(data.Id, data.Catalog, item);
            return item;
        }

        private ICatalogContext CreateCatalogContextFromCatalogRoute(RouteData routeData)
        {
            var currentStorefront = StorefrontManager.CurrentStorefront;
            var productCatalogItem = currentStorefront.HomeItem.Axes.GetDescendant("product catalog/" + routeData.Values["catalogPath"]);
            if (productCatalogItem == null)
            {
                return null;
            }
            return Create(productCatalogItem);
        }

        private string GetCategoryIdFromItem(Item item)
        {
            if (item.IsDerived(Foundation.Commerce.Templates.Commerce.Category.ID))
            {
                return item.Name.ToLowerInvariant();
            }
            if (item.IsDerived(Foundation.Commerce.Templates.Commerce.Product.ID))
            {
                return item.Parent.Name.ToLowerInvariant();
            }
            if (item.IsDerived(Foundation.Commerce.Templates.Commerce.ProductVariant.ID))
            {
                return item.Parent.Parent.Name.ToLowerInvariant();
            }
            return null;
        }
    }
}