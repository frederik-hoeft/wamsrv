using System;
using System.Collections.Generic;
using System.Text;

namespace wamsrv.Config
{
    public class WamsrvDevelopmentConfig
    {
        public readonly bool BlockResponses;
        public WamsrvDevelopmentConfig(bool blockResponses)
        {
            BlockResponses = blockResponses;
        }
    }
}
