namespace Intextwo.Models
{
    public interface IFraudPredictionService
    {
        Task<bool> IsFraudulentOrderAsync(Order order);
    }
}
