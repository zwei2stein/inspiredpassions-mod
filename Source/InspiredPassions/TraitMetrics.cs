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
            return $"G={good}, N={neutral}, B={bad}, DNT={doNotTouch}";
        }
    }
}