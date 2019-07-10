using System;
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
            var tagsEqual = BuildTag == NHM.Common.BUILD_TAG.BuildTag;
            if (!tagsEqual)
            {
                MessageBox.Show($"Build tags differ this build is broken. NiceHashMiner.BUILD_TAG.BuildTag('{BuildTag}') != NHM.Common.BUILD_TAG.BuildTag('{NHM.Common.BUILD_TAG.BuildTag}')",
                        "BROKEN BULD!!!",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw new Exception($"Broken build TAGS don't match!!!");
            }
        }
    }
}
