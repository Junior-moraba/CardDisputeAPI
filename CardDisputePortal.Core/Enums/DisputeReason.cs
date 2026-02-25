using System.ComponentModel;

namespace CardDisputePortal.Core.Enums
{
    public enum DisputeReason
    {
        [Description("Unauthorized Transaction")]
        Unauthorized,

        [Description("Duplicate Charge")]
        Duplicate,

        [Description("Incorrect Amount")]
        IncorrectAmount,

        [Description("Product/Service Not Received")]
        NotReceived,

        [Description("Fraudulent Transaction")]
        Fraudulent,

        [Description("Cancelled Service")]
        Cancelled,

        [Description("OTHER")]
        Other
    }
}