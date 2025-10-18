using System;

namespace XlsWork
{
    public class IndividualData
    {
        public string[] Values;
        public IndividualData(int Columns)
        {
            Values = new string[Columns];
        }
    }
}