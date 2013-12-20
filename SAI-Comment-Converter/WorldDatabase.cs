using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace SAI_Comment_Converter
{
    class WorldDatabase : Database<MySqlConnection, MySqlConnectionStringBuilder, MySqlParameter, MySqlCommand, MySqlTransaction>
    {
        public WorldDatabase(string host, uint port, string username, string password, string databaseName)
        {
            connectionString = new MySqlConnectionStringBuilder();
            connectionString.Server = host;
            connectionString.Port = (uint)port;
            connectionString.UserID = username;
            connectionString.Password = password;
            connectionString.Database = databaseName;
            connectionString.AllowUserVariables = true;
            connectionString.AllowZeroDateTime = true;
        }

        public async Task<int> GetCreatureIdByGuid(int guid)
        {
            DataTable dt = await ExecuteQuery("SELECT id FROM creature WHERE guid = '" + guid + "'");

            if (dt.Rows.Count == 0)
                return 0;

            return Convert.ToInt32(dt.Rows[0]["id"]);
        }

        public async Task<int> GetGameobjectIdByGuid(int guid)
        {
            DataTable dt = await ExecuteQuery("SELECT id FROM gameobject WHERE guid = '" + guid + "'");

            if (dt.Rows.Count == 0)
                return 0;

            return Convert.ToInt32(dt.Rows[0]["id"]);
        }

        public async Task<string> GetCreatureNameById(int id)
        {
            DataTable dt = await ExecuteQuery("SELECT name FROM creature_template WHERE entry = '" + id + "'");

            if (dt.Rows.Count == 0)
                return "_replaceBecauseOfError_ GetCreatureNameById not found";

            string name = dt.Rows[0]["name"].ToString();
            return name.Replace('"', '\'');
        }

        public async Task<string> GetCreatureNameByGuid(int guid)
        {
            DataTable dt = await ExecuteQuery("SELECT name FROM creature_template WHERE entry = '" + await GetCreatureIdByGuid(guid) + "'");

            if (dt.Rows.Count == 0)
                return "_replaceBecauseOfError_ GetCreatureNameByGuid not found";

            string name = dt.Rows[0]["name"].ToString();
            return name.Replace('"', '\'');
        }

        public async Task<string> GetGameobjectNameById(int id)
        {
            DataTable dt = await ExecuteQuery("SELECT name FROM gameobject_template WHERE entry = '" + id + "'");

            if (dt.Rows.Count == 0)
                return "_replaceBecauseOfError_ GetGameobjectNameById not found";

            string name = dt.Rows[0]["name"].ToString();
            return name.Replace('"', '\'');
        }

        public async Task<string> GetGameobjectNameByGuid(int guid)
        {
            DataTable dt = await ExecuteQuery("SELECT name FROM gameobject_template WHERE entry = '" + await GetGameobjectIdByGuid(guid) + "'");

            if (dt.Rows.Count == 0)
                return "_replaceBecauseOfError_ GetGameobjectNameByGuid not found";

            string name = dt.Rows[0]["name"].ToString();
            return name.Replace('"', '\'');
        }

        public async Task<List<SmartScript>> GetSmartScripts()
        {
            DataTable dt = await ExecuteQuery("SELECT * FROM smart_scripts ORDER BY entryorguid");

            if (dt.Rows.Count == 0)
                return null;

            List<SmartScript> smartScripts = new List<SmartScript>();

            foreach (DataRow row in dt.Rows)
                smartScripts.Add(BuildSmartScript(row));

            return smartScripts;
        }

        public async Task<List<SmartScript>> GetSmartScripts(int entryorguid, int source_type)
        {
            DataTable dt = await ExecuteQuery("SELECT * FROM smart_scripts WHERE entryorguid = '" + entryorguid + "' AND source_type = '" + source_type + "'");

            if (dt.Rows.Count == 0)
                return null;

            List<SmartScript> smartScripts = new List<SmartScript>();

            foreach (DataRow row in dt.Rows)
                smartScripts.Add(BuildSmartScript(row));

            return smartScripts;
        }

        public async Task<List<SmartScript>> GetSmartScriptsWithCondition(string condition)
        {
            DataTable dt = await ExecuteQuery("SELECT * FROM smart_scripts " + condition + ";");

            if (dt.Rows.Count == 0)
                return null;

            List<SmartScript> smartScripts = new List<SmartScript>();

            foreach (DataRow row in dt.Rows)
                smartScripts.Add(BuildSmartScript(row));

            return smartScripts;
        }

        public async Task<string> GetSpellNameById(int id)
        {
            DataTable dt = await ExecuteQuery("SELECT spellName FROM spells_dbc WHERE id = " + id);

            if (dt.Rows.Count == 0)
                return "_replaceBecauseOfError_<Spell '" + id + "' not found!>";

            return dt.Rows[0]["spellName"].ToString();
        }

        public async Task<string> GetQuestNameById(int id)
        {
            DataTable dt = await ExecuteQuery("SELECT title FROM quest_template WHERE id = " + id);

            if (dt.Rows.Count == 0)
                return "_replaceBecauseOfError_<Quest '" + id + "' not found!>";

            return dt.Rows[0]["title"].ToString();
        }

        public async Task<string> GetQuestNameForCastedByCreatureOrGo(int requiredNpcOrGo1, int requiredNpcOrGo2, int requiredNpcOrGo3, int requiredNpcOrGo4, int requiredSpellCast1)
        {
            DataTable dt = await ExecuteQuery(String.Format("SELECT title FROM quest_template WHERE (RequiredNpcOrGo1 = {0} OR RequiredNpcOrGo2 = {1} OR RequiredNpcOrGo3 = {2} OR RequiredNpcOrGo4 = {3}) AND RequiredSpellCast1 = {4}", requiredNpcOrGo1, requiredNpcOrGo2, requiredNpcOrGo3, requiredNpcOrGo4, requiredSpellCast1));

            if (dt.Rows.Count == 0)
                return "_replaceBecauseOfError_<Quest not found (GetQuestNameForCastedByCreatureOrGo)!>";

            return dt.Rows[0]["title"].ToString();
        }

        public async Task<string> GetQuestNameForKilledMonster(int requiredNpcOrGo1, int requiredNpcOrGo2, int requiredNpcOrGo3, int requiredNpcOrGo4)
        {
            DataTable dt = await ExecuteQuery(String.Format("SELECT title FROM quest_template WHERE (RequiredNpcOrGo1 = {0} OR RequiredNpcOrGo2 = {1} OR RequiredNpcOrGo3 = {2} OR RequiredNpcOrGo4 = {3})", requiredNpcOrGo1, requiredNpcOrGo2, requiredNpcOrGo3, requiredNpcOrGo4));

            if (dt.Rows.Count == 0)
                return "_replaceBecauseOfError_<Quest not found (GetQuestNameForKilledMonster)!>";

            return dt.Rows[0]["title"].ToString();
        }

        public async Task<string> GetItemNameById(int id)
        {
            DataTable dt = await ExecuteQuery("SELECT name FROM item_template WHERE entry = " + id);

            if (dt.Rows.Count == 0)
                return "_replaceBecauseOfError_<Item '" + id + "' not found!>";

            return dt.Rows[0]["name"].ToString();
        }

        private static SmartScript BuildSmartScript(DataRow row)
        {
            var smartScript = new SmartScript();
            smartScript.entryorguid = row["entryorguid"] != DBNull.Value ? Convert.ToInt32(row["entryorguid"]) : -1;
            smartScript.source_type = row["source_type"] != DBNull.Value ? Convert.ToInt32(row["source_type"]) : 0;
            smartScript.id = row["id"] != DBNull.Value ? Convert.ToInt32(row["id"]) : 0;
            smartScript.link = row["link"] != DBNull.Value ? Convert.ToInt32(row["link"]) : 0;
            smartScript.event_type = row["event_type"] != DBNull.Value ? Convert.ToInt32(row["event_type"]) : 0;
            smartScript.event_phase_mask = row["event_phase_mask"] != DBNull.Value ? Convert.ToInt32(row["event_phase_mask"]) : 0;
            smartScript.event_chance = row["event_chance"] != DBNull.Value ? Convert.ToInt32(row["event_chance"]) : 0;
            smartScript.event_flags = row["event_flags"] != DBNull.Value ? Convert.ToInt32(row["event_flags"]) : 0;
            smartScript.event_param1 = row["event_param1"] != DBNull.Value ? Convert.ToInt32(row["event_param1"]) : 0;
            smartScript.event_param2 = row["event_param2"] != DBNull.Value ? Convert.ToInt32(row["event_param2"]) : 0;
            smartScript.event_param3 = row["event_param3"] != DBNull.Value ? Convert.ToInt32(row["event_param3"]) : 0;
            smartScript.event_param4 = row["event_param4"] != DBNull.Value ? Convert.ToInt32(row["event_param4"]) : 0;
            smartScript.action_type = row["action_type"] != DBNull.Value ? Convert.ToInt32(row["action_type"]) : 0;
            smartScript.action_param1 = row["action_param1"] != DBNull.Value ? Convert.ToInt32(row["action_param1"]) : 0;
            smartScript.action_param2 = row["action_param2"] != DBNull.Value ? Convert.ToInt32(row["action_param2"]) : 0;
            smartScript.action_param3 = row["action_param3"] != DBNull.Value ? Convert.ToInt32(row["action_param3"]) : 0;
            smartScript.action_param4 = row["action_param4"] != DBNull.Value ? Convert.ToInt32(row["action_param4"]) : 0;
            smartScript.action_param5 = row["action_param5"] != DBNull.Value ? Convert.ToInt32(row["action_param5"]) : 0;
            smartScript.action_param6 = row["action_param6"] != DBNull.Value ? Convert.ToInt32(row["action_param6"]) : 0;
            smartScript.target_type = row["target_type"] != DBNull.Value ? Convert.ToInt32(row["target_type"]) : 0;
            smartScript.target_param1 = row["target_param1"] != DBNull.Value ? Convert.ToInt32(row["target_param1"]) : 0;
            smartScript.target_param2 = row["target_param2"] != DBNull.Value ? Convert.ToInt32(row["target_param2"]) : 0;
            smartScript.target_param3 = row["target_param3"] != DBNull.Value ? Convert.ToInt32(row["target_param3"]) : 0;
            smartScript.target_x = row["target_x"] != DBNull.Value ? Convert.ToInt32(row["target_x"]) : 0;
            smartScript.target_y = row["target_y"] != DBNull.Value ? Convert.ToInt32(row["target_y"]) : 0;
            smartScript.target_z = row["target_z"] != DBNull.Value ? Convert.ToInt32(row["target_z"]) : 0;
            smartScript.target_o = row["target_o"] != DBNull.Value ? Convert.ToInt32(row["target_o"]) : 0;
            smartScript.comment = row["comment"] != DBNull.Value ? (string)row["comment"] : String.Empty;
            return smartScript;
        }
    }
}
