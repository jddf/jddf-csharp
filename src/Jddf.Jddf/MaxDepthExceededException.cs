using System;

namespace Jddf.Jddf
{
    public class MaxDepthExceededException : Exception
    {
        public MaxDepthExceededException() : base("max depth exceeded during validation")
        {
        }
    }
}
