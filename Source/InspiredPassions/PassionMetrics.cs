namespace InspiredPassions
{
    public class PassionMetrics
    {
        public int nonePassions;
        public int minorPassions;
        public int majorPassions;
        public int enabledPassionableSkills;
        
        public override string ToString()
        {
            return $"None: {nonePassions}, Minor: {minorPassions}, Major: {majorPassions}, EPS: {enabledPassionableSkills}";
        }
    }
    
}