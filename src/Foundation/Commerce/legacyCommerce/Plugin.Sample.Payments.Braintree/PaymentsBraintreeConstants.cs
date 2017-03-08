﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PaymentsBraintreeConstants.cs" company="Sitecore Corporation">
//   Copyright (c) Sitecore Corporation 1999-2017
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Plugin.Sample.Payments.Braintree
{
    /// <summary>
    /// The payments constants.
    /// </summary>
    public static class PaymentsBraintreeConstants
    {
        /// <summary>
        /// The name of the payments pipelines.
        /// </summary>
        public static class Pipelines
        {
            /// <summary>
            /// The name of the payment pipelines blocks.
            /// </summary>
            public static class Blocks
            {
                /// <summary>
                /// The get client token block name.
                /// </summary>
                public const string GetClientTokenBlock = "PaymentsBraintree.block.getclienttoken";

                /// <summary>
                /// The add federated payment block
                /// </summary>
                public const string UpdateFederatedPaymentBlock = "PaymentsBraintree.block.updatefederatedpayment";

                /// <summary>
                /// The update federated payment after settlement block
                /// </summary>
                public const string UpdateFederatedPaymentAfterSettlementBlock = "PaymentsBraintree.block.updatefederatedpaymentaftersettlement";

                /// <summary>
                /// The create federated payment block
                /// </summary>
                public const string CreateFederatedPaymentBlock = "PaymentsBraintree.block.createfederatedpayment";

                /// <summary>
                /// The ensure settle payment requested block
                /// </summary>
                public const string EnsureSettlePaymentRequestedBlock = "PaymentsBraintree.block.ensuresettlepaymentrequested";

                /// <summary>
                /// The void federated payment block
                /// </summary>
                public const string VoidFederatedPaymentBlock = "PaymentsBraintree.block.voidfederatedpayment";

                /// <summary>
                /// The void cancel order federated payment block
                /// </summary>
                public const string VoidCancelOrderFederatedPaymentBlock = "PaymentsBraintree.block.voidcancelorderfederatedpayment";

                /// <summary>
                /// The refund federated payment block
                /// </summary>
                public const string RefundFederatedPaymentBlock = "PaymentsBraintree.block.refundfederatedpayment";             

                /// <summary>
                /// The void on hold order federated payment block
                /// </summary>
                public const string VoidOnHoldOrderFederatedPaymentBlock = "PaymentsBraintree.block.voidonholdorderfederatedpayment";

                /// <summary>
                /// The validate settlement block
                /// </summary>
                public const string ValidateSettlementBlock = "PaymentsBraintree.block.validatesettlement";
            }
        }
    }
}