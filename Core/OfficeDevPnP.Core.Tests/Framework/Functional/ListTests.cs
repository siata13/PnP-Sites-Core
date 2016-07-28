﻿using Microsoft.SharePoint.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OfficeDevPnP.Core.Framework.Provisioning.Model;
using OfficeDevPnP.Core.Tests.Framework.Functional.Validators;
using System;
using System.Linq;

namespace OfficeDevPnP.Core.Tests.Framework.Functional
{
    [TestClass]
    public class ListTests : FunctionalTestBase
    {
        #region Construction
        public ListTests()
        {
            debugMode = true;
            centralSiteCollectionUrl = "https://bertonline.sharepoint.com/sites/TestPnPSC_12345_ab5f2990-6015-48c5-a09b-685153dcebc9";
            centralSubSiteUrl = "https://bertonline.sharepoint.com/sites/TestPnPSC_12345_ab5f2990-6015-48c5-a09b-685153dcebc9/sub";
        }
        #endregion

        #region Test setup
        [ClassInitialize()]
        public static void ClassInit(TestContext context)
        {
            ClassInitBase(context);
        }

        [ClassCleanup()]
        public static void ClassCleanup()
        {
            ClassCleanupBase();
        }
        #endregion

        #region Site collection test cases
        [TestMethod]
        public void SiteCollectionListAddingTest()
        {
            using (var cc = TestCommon.CreateClientContext(centralSiteCollectionUrl))
            {
                // Ensure we can test clean
                DeleteLists(cc);
                
                // Add lists
                var result = TestProvisioningTemplate(cc, "list_add.xml", Handlers.Lists);
                ListInstanceValidator lv = new ListInstanceValidator();
                Assert.IsTrue(lv.Validate(result.SourceTemplate.Lists, result.TargetTemplate.Lists, result.TargetTokenParser));

            }
        }
        #endregion

        #region Web test cases
        // No need to have these as the engine is blocking creation and extraction of content types at web level
        #endregion

        #region Validation event handlers
        #endregion

        #region Helper methods
        private void DeleteLists(ClientContext cc)
        {
            // delete lists from root web and sub web
            DeleteListsImplementation(cc);

            using (ClientContext cc2 = cc.Clone(centralSubSiteUrl))
            {
                DeleteListsImplementation(cc2);
            }

            // Drop the created content types
            DeleteContentTypes(cc);
        }

        private static void DeleteListsImplementation(ClientContext cc)
        {
            cc.Load(cc.Web.Lists, f => f.Include(t => t.Title));
            cc.ExecuteQueryRetry();

            foreach (var list in cc.Web.Lists.ToList())
            {
                if (list.Title.StartsWith("LI_"))
                {
                    list.DeleteObject();
                }
            }
            cc.ExecuteQueryRetry();
        }

        private void DeleteContentTypes(ClientContext cc)
        {
            // Drop the content types
            cc.Load(cc.Web.ContentTypes, f => f.Include(t => t.Name));
            cc.ExecuteQueryRetry();

            foreach (var ct in cc.Web.ContentTypes.ToList())
            {
                if (ct.Name.StartsWith("CT_"))
                {
                    ct.DeleteObject();
                }
            }
            cc.ExecuteQueryRetry();

            // Drop the fields
            DeleteFields(cc);
        }

        private void DeleteFields(ClientContext cc)
        {
            cc.Load(cc.Web.Fields, f => f.Include(t => t.InternalName));
            cc.ExecuteQueryRetry();

            foreach (var field in cc.Web.Fields.ToList())
            {
                // First drop the fields that have 2 _'s...convention used to name the fields dependent on a lookup.
                if (field.InternalName.Replace("FLD_CT_", "").IndexOf("_") > 0)
                {
                    if (field.InternalName.StartsWith("FLD_CT_"))
                    {
                        field.DeleteObject();
                    }
                }
            }

            foreach (var field in cc.Web.Fields.ToList())
            {
                if (field.InternalName.StartsWith("FLD_CT_"))
                {
                    field.DeleteObject();
                }
            }

            cc.ExecuteQueryRetry();

        }
        #endregion
    }
}
