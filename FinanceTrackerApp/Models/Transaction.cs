namespace FinanceTrackerApp.Models
{
    public enum TxType { Income, Expense }
    public class Transaction
    {
        public int Id { get; set; }
        public TxType Type { get; set; }
        public decimal Amount { get; set; }
        public string Category { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
