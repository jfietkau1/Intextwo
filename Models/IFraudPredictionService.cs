namespace Intextwo.Models
{
    public interface IFraudPredictionService
    {
        bool IsFraudulentOrder(Order order);
    }
}
