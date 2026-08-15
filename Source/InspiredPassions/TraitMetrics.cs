namespace InspiredPassions
{
    public class TraitMetrics
    {
        public int good;
        public int neutral;
        public int bad;
        public int doNotTouch;
        
        public override string ToString()
        {
            return $"Good={good}, Neutral={neutral}, Bad={bad}, DNT={doNotTouch}";
        }
    }
}