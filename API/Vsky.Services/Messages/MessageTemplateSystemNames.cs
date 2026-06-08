namespace Vsky.Services.Messages
{
    public class MessageTemplateSystemNames
    {
        public const string UserWelcomeMessage = "User.WelcomeMessage";

        public const string UserResetPassword = "User.ResetPassword";

        public const string UserChangePassword = "User.ChangePassword";

        public const string UserTwoFactorToken = "User.TwoFactorToken";

        public const string LeaveForwardToken = "Leave.Forward";

        public const string LeaveSendToHRToken = "Leave.SendToHR";

        // Help Desk
        public const string RequestProcessingToken = "HelpDesk.RequestProcessing";
        public const string HelpDeskMailSendToRequester = "HelpDesk.HelpDeskMailSendToRequester";
        // In use
        public const string RequestReceivedToken = "HelpDesk.RequestReceived";
        public const string HelpDeskMailSendToAdminToken = "HelpDesk.HelpDeskMailSendToAdmin";
        public const string HelpDeskStatusMail = "HelpDesk.HelpDeskStatusMail";
        public const string HelpDeskAssignedToMail = "HelpDesk.HelpDeskAssignedToMail";

        // Help Desk Reminder
        public const string TicketUnassignedHours = "HelpDesk.TicketUnassigned";
        public const string MoveAssignedToInProgress = "HelpDesk.MoveAssignedToInProgress";
        public const string InProgressNoActivity = "HelpDesk.InProgressNoActivity";
        public const string CompletedToClosed = "HelpDesk.CompletedToClosed";
        public const string PendingFirstEmailReply = "HelpDesk.PendingFirstEmailReply";

        public const string LeaveApprovalSendToEmployeeToken = "Leave.SendLeaveApprovalToEmployee";
        public const string LeaveStatusSendToEmployeeToken = "Leave.SendLeaveStatusToEmployee";
        public const string SendLeaveCancellationToEmployeeToken = "Leave.SendLeaveCancellationToEmployee";

        public const string WebsiteDemoMailToUserToken = "WebsiteDemo.WebsiteDemoMailToUser";
        public const string WebsiteDemoRequestSubmittedMailToken = "WebsiteDemo.WebsiteDemoRequestSubmitted";

        public const string MissingTimeOutSubmitToken = "TimeInTimeOut.MissingTimeOutApproval";
        public const string TimeOutApprovedEmailToEmployeeToken = "TimeInTimeOut.TimeOutApproved";
        public const string TimeOutDeclinedEmailToEmployeeToken = "TimeInTimeOut.TimeOutDeclined";

        public const string ExpensePreApproveRequestToken = "Expense.ExpensePreApproveRequest";
        public const string ExpenseApproveRequestToken = "Expense.ExpenseApproveRequest";
        public const string ExpensePaymentRequestToken = "Expense.ExpensePayRequest";
        public const string ExpensePreApprovedToken = "Expense.ExpensePreApproved";
        public const string ExpenseApprovedToken = "Expense.ExpenseApproved";
        public const string ExpensePaidToken = "Expense.ExpensePaid";

        public const string AdvanceExpensePreApproveRequestToken = "AdvanceExpense.AdvanceExpensePreApproveRequest";
        public const string AdvanceExpenseApproveRequestToken = "AdvanceExpense.AdvanceExpenseApproveRequest";
        public const string AdvanceExpensePaymentRequestToken = "AdvanceExpense.AdvanceExpensePayRequest";
        public const string AdvanceExpensePreApprovedToken = "AdvanceExpense.AdvanceExpensePreApproved";
        public const string AdvanceExpenseApprovedToken = "AdvanceExpense.AdvanceExpenseApproved";
        public const string AdvanceExpensePaidToken = "AdvanceExpense.AdvanceExpensePaid";

        public const string PurchaseExpensePreApproveRequestToken = "PurchaseExpense.PurchaseExpensePreApproveRequest";
        public const string PurchaseExpenseApproveRequestToken = "PurchaseExpense.PurchaseExpenseApproveRequest";
        public const string PurchaseExpensePaymentRequestToken = "PurchaseExpense.PurchaseExpensePayRequest";
        public const string PurchaseExpensePreApprovedToken = "PurchaseExpense.PurchaseExpensePreApproved";
        public const string PurchaseExpenseApprovedToken = "PurchaseExpense.PurchaseExpenseApproved";
        public const string PurchaseExpensePaidToken = "PurchaseExpense.PurchaseExpensePaid";

        public const string EmployeeAnniversaryToHRToken = "Employee.EmployeeAnniversaryToHR";

        // Share My Tenant
        public const string SiteShareInvitationToken = "SiteShare.SiteShareInvitation";
        
        //public const string UserVskyToken = "User.Vsky";
    }
}