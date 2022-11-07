using Newtonsoft.Json;
using Topo.Models.ReportGeneration;
using TopoReportFunction;

namespace TopoReportFunctionTest
{
    [TestClass]
    public class MemberList
    {
        private ReportGenerationRequest _reportGenerationRequest;
        private Function _function;
        private string _reportDataWithLeaders = "";

        [TestInitialize]
        public void SetUp()
        {
            _function = new Function();
            _reportGenerationRequest = new ReportGenerationRequest()
            {
                ReportType = ReportType.MemberList,
                GroupName = "Epping Scout Group",
                Section = "scout",
                UnitName = "Scout Unit",
                OutputType = OutputType.PDF,
                ReportData = "[{\"id\":\"d4bf9481-ee31-3a51-8c5c-ffa8018b7682\",\"member_number\":\"1159894\",\"first_name\":\"Amelia\",\"last_name\":\"Chung\",\"status\":\"active\",\"age\":\"12y 7m\",\"unit_council\":false,\"patrol_name\":\"Drop Bear\",\"patrol_duty\":\"\",\"patrol_order\":3,\"isAdultLeader\":0},{\"id\":\"4ca98009-d4f0-3f04-a9ea-6907d17fe99f\",\"member_number\":\"1131282\",\"first_name\":\"Annabella\",\"last_name\":\"Ronfeldt\",\"status\":\"active\",\"age\":\"13y 4m\",\"unit_council\":true,\"patrol_name\":\"Drop Bear\",\"patrol_duty\":\"APL\",\"patrol_order\":2,\"isAdultLeader\":0},{\"id\":\"ee791361-6a3c-3db5-b7ff-1301283fa803\",\"member_number\":\"1125790\",\"first_name\":\"Chloe\",\"last_name\":\"Small\",\"status\":\"active\",\"age\":\"11y 6m\",\"unit_council\":false,\"patrol_name\":\"Drop Bear\",\"patrol_duty\":\"\",\"patrol_order\":3,\"isAdultLeader\":0},{\"id\":\"31107ada-9841-3575-8429-8365235c0f42\",\"member_number\":\"1103159\",\"first_name\":\"Ethan\",\"last_name\":\"Lloyd\",\"status\":\"active\",\"age\":\"14y 9m\",\"unit_council\":false,\"patrol_name\":\"Drop Bear\",\"patrol_duty\":\"\",\"patrol_order\":3,\"isAdultLeader\":0},{\"id\":\"8d39e613-c1d4-3028-8302-5c32f0bc1e65\",\"member_number\":\"1157492\",\"first_name\":\"James\",\"last_name\":\"Alchin\",\"status\":\"active\",\"age\":\"13y 1m\",\"unit_council\":false,\"patrol_name\":\"Drop Bear\",\"patrol_duty\":\"\",\"patrol_order\":3,\"isAdultLeader\":0},{\"id\":\"90505b04-61f6-3892-81de-a76544a4e460\",\"member_number\":\"1102291\",\"first_name\":\"Zara\",\"last_name\":\"Williams\",\"status\":\"active\",\"age\":\"12y 2m\",\"unit_council\":true,\"patrol_name\":\"Drop Bear\",\"patrol_duty\":\"PL\",\"patrol_order\":1,\"isAdultLeader\":0},{\"id\":\"ba75e5a4-9e57-3cf5-ad40-2e927935622c\",\"member_number\":\"1105699\",\"first_name\":\"Bethany\",\"last_name\":\"Wong\",\"status\":\"active\",\"age\":\"13y 9m\",\"unit_council\":false,\"patrol_name\":\"Lyrebird\",\"patrol_duty\":\"\",\"patrol_order\":3,\"isAdultLeader\":0},{\"id\":\"8e9a6aba-f3e6-3ef2-beaf-5eed0688dd3b\",\"member_number\":\"1059670\",\"first_name\":\"Eric\",\"last_name\":\"Liu\",\"status\":\"active\",\"age\":\"14y 3m\",\"unit_council\":true,\"patrol_name\":\"Lyrebird\",\"patrol_duty\":\"PL\",\"patrol_order\":1,\"isAdultLeader\":0},{\"id\":\"4fadd3a9-4c17-348a-87fd-9cfe93bbd898\",\"member_number\":\"1071573\",\"first_name\":\"Lukas\",\"last_name\":\"Dunn\",\"status\":\"active\",\"age\":\"12y 0m\",\"unit_council\":false,\"patrol_name\":\"Lyrebird\",\"patrol_duty\":\"\",\"patrol_order\":3,\"isAdultLeader\":0},{\"id\":\"e228a1b5-084e-3156-9666-aabff9ed0808\",\"member_number\":\"1159648\",\"first_name\":\"Peter\",\"last_name\":\"Xia\",\"status\":\"active\",\"age\":\"12y 4m\",\"unit_council\":false,\"patrol_name\":\"Lyrebird\",\"patrol_duty\":\"\",\"patrol_order\":3,\"isAdultLeader\":0},{\"id\":\"7e3aeaf6-2248-35e9-8fbe-e7239300e3d4\",\"member_number\":\"1038582\",\"first_name\":\"Sebastien\",\"last_name\":\"Wookey\",\"status\":\"active\",\"age\":\"13y 5m\",\"unit_council\":true,\"patrol_name\":\"Lyrebird\",\"patrol_duty\":\"APL\",\"patrol_order\":2,\"isAdultLeader\":0},{\"id\":\"1f6ade3b-4280-3d68-b3ac-3decdcc5073d\",\"member_number\":\"1100542\",\"first_name\":\"Zoe\",\"last_name\":\"Lloyd\",\"status\":\"active\",\"age\":\"12y 1m\",\"unit_council\":false,\"patrol_name\":\"Lyrebird\",\"patrol_duty\":\"\",\"patrol_order\":3,\"isAdultLeader\":0},{\"id\":\"876ed453-8e89-3e1c-a689-59aed29d6a7c\",\"member_number\":\"1159891\",\"first_name\":\"Daniel\",\"last_name\":\"Chung\",\"status\":\"active\",\"age\":\"14y 1m\",\"unit_council\":false,\"patrol_name\":\"Magpie\",\"patrol_duty\":\"\",\"patrol_order\":3,\"isAdultLeader\":0},{\"id\":\"a6857915-5bd0-3d96-9016-05e7e435dfce\",\"member_number\":\"1105152\",\"first_name\":\"Jeremy\",\"last_name\":\"Chim\",\"status\":\"active\",\"age\":\"11y 6m\",\"unit_council\":false,\"patrol_name\":\"Magpie\",\"patrol_duty\":\"\",\"patrol_order\":3,\"isAdultLeader\":0},{\"id\":\"ede8d90f-2ec6-3a66-b2fa-794f8a6a318a\",\"member_number\":\"1154716\",\"first_name\":\"Ronit\",\"last_name\":\"Natarajan\",\"status\":\"active\",\"age\":\"11y 4m\",\"unit_council\":false,\"patrol_name\":\"Magpie\",\"patrol_duty\":\"\",\"patrol_order\":3,\"isAdultLeader\":0},{\"id\":\"aac9e4d8-b68f-312f-a0a8-8f0e6cfb0c54\",\"member_number\":\"1059661\",\"first_name\":\"Tynn\",\"last_name\":\"Yam\",\"status\":\"active\",\"age\":\"12y 9m\",\"unit_council\":true,\"patrol_name\":\"Magpie\",\"patrol_duty\":\"PL\",\"patrol_order\":1,\"isAdultLeader\":0},{\"id\":\"6e7c548c-0f67-3061-96e9-7d2844cd972c\",\"member_number\":\"1044135\",\"first_name\":\"James\",\"last_name\":\"Li\",\"status\":\"active\",\"age\":\"13y 1m\",\"unit_council\":true,\"patrol_name\":\"Platypus\",\"patrol_duty\":\"APL\",\"patrol_order\":2,\"isAdultLeader\":0},{\"id\":\"5ad97f79-ff0a-3e2f-abe0-94d5e3515ac2\",\"member_number\":\"1102300\",\"first_name\":\"Jonathan\",\"last_name\":\"Li\",\"status\":\"active\",\"age\":\"12y 10m\",\"unit_council\":false,\"patrol_name\":\"Platypus\",\"patrol_duty\":\"\",\"patrol_order\":3,\"isAdultLeader\":0},{\"id\":\"cd1929ca-d2a7-3922-b643-2c7678527d77\",\"member_number\":\"1125788\",\"first_name\":\"Joshua\",\"last_name\":\"Jones\",\"status\":\"active\",\"age\":\"11y 11m\",\"unit_council\":false,\"patrol_name\":\"Platypus\",\"patrol_duty\":\"\",\"patrol_order\":3,\"isAdultLeader\":0},{\"id\":\"66c977e8-bf59-31a1-91d9-69459a0d199c\",\"member_number\":\"1071576\",\"first_name\":\"Phoenix\",\"last_name\":\"Cao\",\"status\":\"active\",\"age\":\"12y 5m\",\"unit_council\":false,\"patrol_name\":\"Platypus\",\"patrol_duty\":\"\",\"patrol_order\":3,\"isAdultLeader\":0},{\"id\":\"4d41f446-9963-3ffd-a677-1fd4eef84cb5\",\"member_number\":\"1102294\",\"first_name\":\"Zoe\",\"last_name\":\"Williams\",\"status\":\"active\",\"age\":\"12y 2m\",\"unit_council\":true,\"patrol_name\":\"Platypus\",\"patrol_duty\":\"PL\",\"patrol_order\":1,\"isAdultLeader\":0},{\"id\":\"bdffb81e-6980-300c-afa0-539fd43831bf\",\"member_number\":\"1113946\",\"first_name\":\"Katrina\",\"last_name\":\"Koop\",\"status\":\"active\",\"age\":\"14y 1m\",\"unit_council\":true,\"patrol_name\":\"Scout Unit Council\",\"patrol_duty\":\"UL\",\"patrol_order\":0,\"isAdultLeader\":0},{\"id\":\"f96da6cf-0a66-303d-94bb-66366dc0ca4a\",\"member_number\":\"1107288\",\"first_name\":\"Alexander\",\"last_name\":\"Holowinski\",\"status\":\"active\",\"age\":\"14y 2m\",\"unit_council\":true,\"patrol_name\":\"Tassie Tigers\",\"patrol_duty\":\"PL\",\"patrol_order\":1,\"isAdultLeader\":0},{\"id\":\"97aaa9bb-2943-3e24-8067-8f1b14df48cd\",\"member_number\":\"1126207\",\"first_name\":\"Cathy\",\"last_name\":\"Graham\",\"status\":\"active\",\"age\":\"13y 11m\",\"unit_council\":true,\"patrol_name\":\"Tassie Tigers\",\"patrol_duty\":\"APL\",\"patrol_order\":2,\"isAdultLeader\":0},{\"id\":\"5c1ad6a1-90db-346f-923f-964a02187b0a\",\"member_number\":\"1104233\",\"first_name\":\"Johnny\",\"last_name\":\"Li\",\"status\":\"active\",\"age\":\"12y 11m\",\"unit_council\":false,\"patrol_name\":\"Tassie Tigers\",\"patrol_duty\":\"\",\"patrol_order\":3,\"isAdultLeader\":0},{\"id\":\"9ff9237a-bc06-3c72-904d-562d2d48ab6c\",\"member_number\":\"1154517\",\"first_name\":\"Matthew\",\"last_name\":\"Last\",\"status\":\"active\",\"age\":\"11y 5m\",\"unit_council\":false,\"patrol_name\":\"Tassie Tigers\",\"patrol_duty\":\"\",\"patrol_order\":3,\"isAdultLeader\":0},{\"id\":\"5d122dd4-2b4a-36d3-b305-3a802c9cae99\",\"member_number\":\"1143210\",\"first_name\":\"Yang\",\"last_name\":\"Pan\",\"status\":\"active\",\"age\":\"14y 3m\",\"unit_council\":false,\"patrol_name\":\"Tassie Tigers\",\"patrol_duty\":\"\",\"patrol_order\":3,\"isAdultLeader\":0}]"
            };
            _reportDataWithLeaders = "[{\"id\":\"2feeb3c1-bfd6-3924-9986-6859357e779e\",\"member_number\":\"1030990\",\"first_name\":\"Alice\",\"last_name\":\"Chaffey\",\"status\":\"active\",\"age\":\"48y 0m\",\"unit_council\":true,\"patrol_name\":\"Adult Leaders - Scouts\",\"patrol_duty\":\"SL\",\"patrol_order\":3,\"isAdultLeader\":1},{\"id\":\"f94b8017-e1d3-3b91-90e5-66d6f8783c97\",\"member_number\":\"1106266\",\"first_name\":\"Hannah\",\"last_name\":\"Chim\",\"status\":\"active\",\"age\":\"48y 3m\",\"unit_council\":true,\"patrol_name\":\"Adult Leaders - Scouts\",\"patrol_duty\":\"SL\",\"patrol_order\":3,\"isAdultLeader\":1},{\"id\":\"2a9c3839-ec8b-3b75-ba9d-1679735b5e56\",\"member_number\":\"1116815\",\"first_name\":\"Kin Chi\",\"last_name\":\"Ling\",\"status\":\"active\",\"age\":\"55y 3m\",\"unit_council\":true,\"patrol_name\":\"Adult Leaders - Scouts\",\"patrol_duty\":\"SL\",\"patrol_order\":3,\"isAdultLeader\":1},{\"id\":\"0ad3922d-853c-3d19-b2c6-62649e82cd7c\",\"member_number\":\"214114\",\"first_name\":\"Mathew\",\"last_name\":\"Lim\",\"status\":\"active\",\"age\":\"59y 5m\",\"unit_council\":true,\"patrol_name\":\"Adult Leaders - Scouts\",\"patrol_duty\":\"SL\",\"patrol_order\":3,\"isAdultLeader\":1},{\"id\":\"fb1e136b-75fc-3d4a-8330-090405e402f0\",\"member_number\":\"190932\",\"first_name\":\"Noriko\",\"last_name\":\"Burrows\",\"status\":\"active\",\"age\":\"57y 9m\",\"unit_council\":true,\"patrol_name\":\"Adult Leaders - Scouts\",\"patrol_duty\":\"SL\",\"patrol_order\":3,\"isAdultLeader\":1},{\"id\":\"e2abe8e1-7c89-3281-862d-baf064f33d3c\",\"member_number\":\"550895\",\"first_name\":\"Peter\",\"last_name\":\"Buckley\",\"status\":\"active\",\"age\":\"66y 11m\",\"unit_council\":true,\"patrol_name\":\"Adult Leaders - Scouts\",\"patrol_duty\":\"SL\",\"patrol_order\":3,\"isAdultLeader\":1},{\"id\":\"f45c571c-2dca-3538-8c13-5783f83643cf\",\"member_number\":\"223378\",\"first_name\":\"Simon\",\"last_name\":\"Batson\",\"status\":\"active\",\"age\":\"60y 9m\",\"unit_council\":true,\"patrol_name\":\"Adult Leaders - Scouts\",\"patrol_duty\":\"SL\",\"patrol_order\":3,\"isAdultLeader\":1},{\"id\":\"4ce3a7c4-b98f-3062-8779-6ebd619ec4e3\",\"member_number\":\"169651\",\"first_name\":\"Tymon\",\"last_name\":\"Domanko\",\"status\":\"active\",\"age\":\"25y 10m\",\"unit_council\":true,\"patrol_name\":\"Adult Leaders - Scouts\",\"patrol_duty\":\"SL\",\"patrol_order\":3,\"isAdultLeader\":1},{\"id\":\"d4bf9481-ee31-3a51-8c5c-ffa8018b7682\",\"member_number\":\"1159894\",\"first_name\":\"Amelia\",\"last_name\":\"Chung\",\"status\":\"active\",\"age\":\"12y 7m\",\"unit_council\":false,\"patrol_name\":\"Drop Bear\",\"patrol_duty\":\"\",\"patrol_order\":3,\"isAdultLeader\":0},{\"id\":\"4ca98009-d4f0-3f04-a9ea-6907d17fe99f\",\"member_number\":\"1131282\",\"first_name\":\"Annabella\",\"last_name\":\"Ronfeldt\",\"status\":\"active\",\"age\":\"13y 4m\",\"unit_council\":true,\"patrol_name\":\"Drop Bear\",\"patrol_duty\":\"APL\",\"patrol_order\":2,\"isAdultLeader\":0},{\"id\":\"ee791361-6a3c-3db5-b7ff-1301283fa803\",\"member_number\":\"1125790\",\"first_name\":\"Chloe\",\"last_name\":\"Small\",\"status\":\"active\",\"age\":\"11y 6m\",\"unit_council\":false,\"patrol_name\":\"Drop Bear\",\"patrol_duty\":\"\",\"patrol_order\":3,\"isAdultLeader\":0},{\"id\":\"31107ada-9841-3575-8429-8365235c0f42\",\"member_number\":\"1103159\",\"first_name\":\"Ethan\",\"last_name\":\"Lloyd\",\"status\":\"active\",\"age\":\"14y 9m\",\"unit_council\":false,\"patrol_name\":\"Drop Bear\",\"patrol_duty\":\"\",\"patrol_order\":3,\"isAdultLeader\":0},{\"id\":\"8d39e613-c1d4-3028-8302-5c32f0bc1e65\",\"member_number\":\"1157492\",\"first_name\":\"James\",\"last_name\":\"Alchin\",\"status\":\"active\",\"age\":\"13y 1m\",\"unit_council\":false,\"patrol_name\":\"Drop Bear\",\"patrol_duty\":\"\",\"patrol_order\":3,\"isAdultLeader\":0},{\"id\":\"90505b04-61f6-3892-81de-a76544a4e460\",\"member_number\":\"1102291\",\"first_name\":\"Zara\",\"last_name\":\"Williams\",\"status\":\"active\",\"age\":\"12y 2m\",\"unit_council\":true,\"patrol_name\":\"Drop Bear\",\"patrol_duty\":\"PL\",\"patrol_order\":1,\"isAdultLeader\":0},{\"id\":\"ba75e5a4-9e57-3cf5-ad40-2e927935622c\",\"member_number\":\"1105699\",\"first_name\":\"Bethany\",\"last_name\":\"Wong\",\"status\":\"active\",\"age\":\"13y 9m\",\"unit_council\":false,\"patrol_name\":\"Lyrebird\",\"patrol_duty\":\"\",\"patrol_order\":3,\"isAdultLeader\":0},{\"id\":\"8e9a6aba-f3e6-3ef2-beaf-5eed0688dd3b\",\"member_number\":\"1059670\",\"first_name\":\"Eric\",\"last_name\":\"Liu\",\"status\":\"active\",\"age\":\"14y 3m\",\"unit_council\":true,\"patrol_name\":\"Lyrebird\",\"patrol_duty\":\"PL\",\"patrol_order\":1,\"isAdultLeader\":0},{\"id\":\"4fadd3a9-4c17-348a-87fd-9cfe93bbd898\",\"member_number\":\"1071573\",\"first_name\":\"Lukas\",\"last_name\":\"Dunn\",\"status\":\"active\",\"age\":\"12y 0m\",\"unit_council\":false,\"patrol_name\":\"Lyrebird\",\"patrol_duty\":\"\",\"patrol_order\":3,\"isAdultLeader\":0},{\"id\":\"e228a1b5-084e-3156-9666-aabff9ed0808\",\"member_number\":\"1159648\",\"first_name\":\"Peter\",\"last_name\":\"Xia\",\"status\":\"active\",\"age\":\"12y 4m\",\"unit_council\":false,\"patrol_name\":\"Lyrebird\",\"patrol_duty\":\"\",\"patrol_order\":3,\"isAdultLeader\":0},{\"id\":\"7e3aeaf6-2248-35e9-8fbe-e7239300e3d4\",\"member_number\":\"1038582\",\"first_name\":\"Sebastien\",\"last_name\":\"Wookey\",\"status\":\"active\",\"age\":\"13y 5m\",\"unit_council\":true,\"patrol_name\":\"Lyrebird\",\"patrol_duty\":\"APL\",\"patrol_order\":2,\"isAdultLeader\":0},{\"id\":\"1f6ade3b-4280-3d68-b3ac-3decdcc5073d\",\"member_number\":\"1100542\",\"first_name\":\"Zoe\",\"last_name\":\"Lloyd\",\"status\":\"active\",\"age\":\"12y 1m\",\"unit_council\":false,\"patrol_name\":\"Lyrebird\",\"patrol_duty\":\"\",\"patrol_order\":3,\"isAdultLeader\":0},{\"id\":\"876ed453-8e89-3e1c-a689-59aed29d6a7c\",\"member_number\":\"1159891\",\"first_name\":\"Daniel\",\"last_name\":\"Chung\",\"status\":\"active\",\"age\":\"14y 1m\",\"unit_council\":false,\"patrol_name\":\"Magpie\",\"patrol_duty\":\"\",\"patrol_order\":3,\"isAdultLeader\":0},{\"id\":\"a6857915-5bd0-3d96-9016-05e7e435dfce\",\"member_number\":\"1105152\",\"first_name\":\"Jeremy\",\"last_name\":\"Chim\",\"status\":\"active\",\"age\":\"11y 6m\",\"unit_council\":false,\"patrol_name\":\"Magpie\",\"patrol_duty\":\"\",\"patrol_order\":3,\"isAdultLeader\":0},{\"id\":\"ede8d90f-2ec6-3a66-b2fa-794f8a6a318a\",\"member_number\":\"1154716\",\"first_name\":\"Ronit\",\"last_name\":\"Natarajan\",\"status\":\"active\",\"age\":\"11y 4m\",\"unit_council\":false,\"patrol_name\":\"Magpie\",\"patrol_duty\":\"\",\"patrol_order\":3,\"isAdultLeader\":0},{\"id\":\"aac9e4d8-b68f-312f-a0a8-8f0e6cfb0c54\",\"member_number\":\"1059661\",\"first_name\":\"Tynn\",\"last_name\":\"Yam\",\"status\":\"active\",\"age\":\"12y 9m\",\"unit_council\":true,\"patrol_name\":\"Magpie\",\"patrol_duty\":\"PL\",\"patrol_order\":1,\"isAdultLeader\":0},{\"id\":\"6e7c548c-0f67-3061-96e9-7d2844cd972c\",\"member_number\":\"1044135\",\"first_name\":\"James\",\"last_name\":\"Li\",\"status\":\"active\",\"age\":\"13y 1m\",\"unit_council\":true,\"patrol_name\":\"Platypus\",\"patrol_duty\":\"APL\",\"patrol_order\":2,\"isAdultLeader\":0},{\"id\":\"5ad97f79-ff0a-3e2f-abe0-94d5e3515ac2\",\"member_number\":\"1102300\",\"first_name\":\"Jonathan\",\"last_name\":\"Li\",\"status\":\"active\",\"age\":\"12y 10m\",\"unit_council\":false,\"patrol_name\":\"Platypus\",\"patrol_duty\":\"\",\"patrol_order\":3,\"isAdultLeader\":0},{\"id\":\"cd1929ca-d2a7-3922-b643-2c7678527d77\",\"member_number\":\"1125788\",\"first_name\":\"Joshua\",\"last_name\":\"Jones\",\"status\":\"active\",\"age\":\"11y 11m\",\"unit_council\":false,\"patrol_name\":\"Platypus\",\"patrol_duty\":\"\",\"patrol_order\":3,\"isAdultLeader\":0},{\"id\":\"66c977e8-bf59-31a1-91d9-69459a0d199c\",\"member_number\":\"1071576\",\"first_name\":\"Phoenix\",\"last_name\":\"Cao\",\"status\":\"active\",\"age\":\"12y 5m\",\"unit_council\":false,\"patrol_name\":\"Platypus\",\"patrol_duty\":\"\",\"patrol_order\":3,\"isAdultLeader\":0},{\"id\":\"4d41f446-9963-3ffd-a677-1fd4eef84cb5\",\"member_number\":\"1102294\",\"first_name\":\"Zoe\",\"last_name\":\"Williams\",\"status\":\"active\",\"age\":\"12y 2m\",\"unit_council\":true,\"patrol_name\":\"Platypus\",\"patrol_duty\":\"PL\",\"patrol_order\":1,\"isAdultLeader\":0},{\"id\":\"bdffb81e-6980-300c-afa0-539fd43831bf\",\"member_number\":\"1113946\",\"first_name\":\"Katrina\",\"last_name\":\"Koop\",\"status\":\"active\",\"age\":\"14y 1m\",\"unit_council\":true,\"patrol_name\":\"Scout Unit Council\",\"patrol_duty\":\"UL\",\"patrol_order\":0,\"isAdultLeader\":0},{\"id\":\"f96da6cf-0a66-303d-94bb-66366dc0ca4a\",\"member_number\":\"1107288\",\"first_name\":\"Alexander\",\"last_name\":\"Holowinski\",\"status\":\"active\",\"age\":\"14y 2m\",\"unit_council\":true,\"patrol_name\":\"Tassie Tigers\",\"patrol_duty\":\"PL\",\"patrol_order\":1,\"isAdultLeader\":0},{\"id\":\"97aaa9bb-2943-3e24-8067-8f1b14df48cd\",\"member_number\":\"1126207\",\"first_name\":\"Cathy\",\"last_name\":\"Graham\",\"status\":\"active\",\"age\":\"13y 11m\",\"unit_council\":true,\"patrol_name\":\"Tassie Tigers\",\"patrol_duty\":\"APL\",\"patrol_order\":2,\"isAdultLeader\":0},{\"id\":\"5c1ad6a1-90db-346f-923f-964a02187b0a\",\"member_number\":\"1104233\",\"first_name\":\"Johnny\",\"last_name\":\"Li\",\"status\":\"active\",\"age\":\"12y 11m\",\"unit_council\":false,\"patrol_name\":\"Tassie Tigers\",\"patrol_duty\":\"\",\"patrol_order\":3,\"isAdultLeader\":0},{\"id\":\"9ff9237a-bc06-3c72-904d-562d2d48ab6c\",\"member_number\":\"1154517\",\"first_name\":\"Matthew\",\"last_name\":\"Last\",\"status\":\"active\",\"age\":\"11y 5m\",\"unit_council\":false,\"patrol_name\":\"Tassie Tigers\",\"patrol_duty\":\"\",\"patrol_order\":3,\"isAdultLeader\":0},{\"id\":\"5d122dd4-2b4a-36d3-b305-3a802c9cae99\",\"member_number\":\"1143210\",\"first_name\":\"Yang\",\"last_name\":\"Pan\",\"status\":\"active\",\"age\":\"14y 3m\",\"unit_council\":false,\"patrol_name\":\"Tassie Tigers\",\"patrol_duty\":\"\",\"patrol_order\":3,\"isAdultLeader\":0}]";
            if (!Directory.Exists("TestResults"))
                Directory.CreateDirectory("TestResults");
        }

        [TestMethod]
        public void MemberList_ToPDF()
        {
            var proxyRequest = new Amazon.Lambda.APIGatewayEvents.APIGatewayProxyRequest();
            proxyRequest.Body = JsonConvert.SerializeObject(_reportGenerationRequest);
            var functionResult = _function.FunctionHandler(proxyRequest, null);
            //Convert Base64String into PDF document
            byte[] bytes = Convert.FromBase64String(functionResult);
            FileStream fileStream = new FileStream(@"TestResults\MemberList_ToPDF.pdf", FileMode.Create);
            BinaryWriter writer = new BinaryWriter(fileStream);
            writer.Write(bytes, 0, bytes.Length);
            writer.Close();
        }

        [TestMethod]
        public void MemberList_ToExcel()
        {
            _reportGenerationRequest.OutputType = OutputType.Excel;
            var proxyRequest = new Amazon.Lambda.APIGatewayEvents.APIGatewayProxyRequest();
            proxyRequest.Body = JsonConvert.SerializeObject(_reportGenerationRequest);
            var functionResult = _function.FunctionHandler(proxyRequest, null);
            //Convert Base64String into PDF document
            byte[] bytes = Convert.FromBase64String(functionResult);
            FileStream fileStream = new FileStream(@"TestResults\MemberList_ToExcel.xlsx", FileMode.Create);
            BinaryWriter writer = new BinaryWriter(fileStream);
            writer.Write(bytes, 0, bytes.Length);
            writer.Close();
        }

        [TestMethod]
        public void PatrolList_ToPDF_IncludeLeaders_False()
        {
            _reportGenerationRequest.ReportType = ReportType.PatrolList;
            _reportGenerationRequest.IncludeLeaders = false;
            var proxyRequest = new Amazon.Lambda.APIGatewayEvents.APIGatewayProxyRequest();
            proxyRequest.Body = JsonConvert.SerializeObject(_reportGenerationRequest);
            var functionResult = _function.FunctionHandler(proxyRequest, null);
            //Convert Base64String into PDF document
            byte[] bytes = Convert.FromBase64String(functionResult);
            FileStream fileStream = new FileStream(@"TestResults\PatrolList_ToPDF_IncludeLeaders_False.pdf", FileMode.Create);
            BinaryWriter writer = new BinaryWriter(fileStream);
            writer.Write(bytes, 0, bytes.Length);
            writer.Close();
        }

        [TestMethod]
        public void PatrolList_ToExcel_IncludeLeaders_False()
        {
            _reportGenerationRequest.ReportType = ReportType.PatrolList;
            _reportGenerationRequest.IncludeLeaders = false;
            _reportGenerationRequest.OutputType = OutputType.Excel;
            var proxyRequest = new Amazon.Lambda.APIGatewayEvents.APIGatewayProxyRequest();
            proxyRequest.Body = JsonConvert.SerializeObject(_reportGenerationRequest);
            var functionResult = _function.FunctionHandler(proxyRequest, null);
            //Convert Base64String into PDF document
            byte[] bytes = Convert.FromBase64String(functionResult);
            FileStream fileStream = new FileStream(@"TestResults\PatrolList_ToExcel_IncludeLeaders_False.xlsx", FileMode.Create);
            BinaryWriter writer = new BinaryWriter(fileStream);
            writer.Write(bytes, 0, bytes.Length);
            writer.Close();
        }

        [TestMethod]
        public void PatrolList_ToPDF_IncludeLeaders_True()
        {
            _reportGenerationRequest.ReportType = ReportType.PatrolList;
            _reportGenerationRequest.IncludeLeaders = true;
            _reportGenerationRequest.ReportData = _reportDataWithLeaders;
            var proxyRequest = new Amazon.Lambda.APIGatewayEvents.APIGatewayProxyRequest();
            proxyRequest.Body = JsonConvert.SerializeObject(_reportGenerationRequest);
            var functionResult = _function.FunctionHandler(proxyRequest, null);
            //Convert Base64String into PDF document
            byte[] bytes = Convert.FromBase64String(functionResult);
            FileStream fileStream = new FileStream(@"TestResults\PatrolList_ToPDF_IncludeLeaders_True.pdf", FileMode.Create);
            BinaryWriter writer = new BinaryWriter(fileStream);
            writer.Write(bytes, 0, bytes.Length);
            writer.Close();
        }

        [TestMethod]
        public void PatrolList_ToExcel_IncludeLeaders_True()
        {
            _reportGenerationRequest.ReportType = ReportType.PatrolList;
            _reportGenerationRequest.IncludeLeaders = true;
            _reportGenerationRequest.ReportData = _reportDataWithLeaders;
            _reportGenerationRequest.OutputType = OutputType.Excel;
            var proxyRequest = new Amazon.Lambda.APIGatewayEvents.APIGatewayProxyRequest();
            proxyRequest.Body = JsonConvert.SerializeObject(_reportGenerationRequest);
            var functionResult = _function.FunctionHandler(proxyRequest, null);
            //Convert Base64String into PDF document
            byte[] bytes = Convert.FromBase64String(functionResult);
            FileStream fileStream = new FileStream(@"TestResults\PatrolList_ToExcel_IncludeLeaders_True.xlsx", FileMode.Create);
            BinaryWriter writer = new BinaryWriter(fileStream);
            writer.Write(bytes, 0, bytes.Length);
            writer.Close();
        }

        [TestMethod]
        public void PatrolSheets_ToPDF()
        {
            _reportGenerationRequest.ReportType = ReportType.PatrolSheets;
            var proxyRequest = new Amazon.Lambda.APIGatewayEvents.APIGatewayProxyRequest();
            proxyRequest.Body = JsonConvert.SerializeObject(_reportGenerationRequest);
            var functionResult = _function.FunctionHandler(proxyRequest, null);
            //Convert Base64String into PDF document
            byte[] bytes = Convert.FromBase64String(functionResult);
            FileStream fileStream = new FileStream(@"TestResults\PatrolSheets_ToPDF.pdf", FileMode.Create);
            BinaryWriter writer = new BinaryWriter(fileStream);
            writer.Write(bytes, 0, bytes.Length);
            writer.Close();
        }

        [TestMethod]
        public void PatrolSheets_ToExcel()
        {
            _reportGenerationRequest.ReportType = ReportType.PatrolSheets;
            _reportGenerationRequest.OutputType = OutputType.Excel;
            var proxyRequest = new Amazon.Lambda.APIGatewayEvents.APIGatewayProxyRequest();
            proxyRequest.Body = JsonConvert.SerializeObject(_reportGenerationRequest);
            var functionResult = _function.FunctionHandler(proxyRequest, null);
            //Convert Base64String into PDF document
            byte[] bytes = Convert.FromBase64String(functionResult);
            FileStream fileStream = new FileStream(@"TestResults\PatrolSheets_ToPDF.xlsx", FileMode.Create);
            BinaryWriter writer = new BinaryWriter(fileStream);
            writer.Write(bytes, 0, bytes.Length);
            writer.Close();
        }
    }
}
