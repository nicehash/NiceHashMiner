using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NiceHashMiner
{
    public static class BUILD_TAG
    {
#if TESTNET
        private static readonly string BuildTag = "TESTNET";
#elif TESTNETDEV
        private static readonly string BuildTag = "TESTNETDEV";
#elif PRODUCTION_NEW
        private static readonly string BuildTag = "PRODUCTION_NEW";
#else
        private static readonly string BuildTag = "PRODUCTION";
#endif

        public static void ASSERT_COMPATIBLE_BUILDS()
        {
            var tagsEqual = BuildTag == NiceHashMinerLegacy.Common.BUILD_TAG.BuildTag;
            if (!tagsEqual)
            {
                MessageBox.Show($"Build tags differ this build is broken. NiceHashMiner.BUILD_TAG.BuildTag('{BuildTag}') != NiceHashMinerLegacy.Common.BUILD_TAG.BuildTag('{NiceHashMinerLegacy.Common.BUILD_TAG.BuildTag}')",
                        "BROKEN BULD!!!",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw new Exception($"Broken build TAGS don't match!!!");
            }
        }
    }
}
