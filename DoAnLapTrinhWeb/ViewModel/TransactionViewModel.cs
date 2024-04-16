using DoAnLapTrinhWeb.Models;

namespace DoAnLapTrinhWeb.ViewModels
{
    public class TransactionViewModel
    {
        public IEnumerable<Transaction> IncomeTransactions { get; set; }
        public IEnumerable<Transaction> ExpenseTransactions { get; set; }
    }
}
