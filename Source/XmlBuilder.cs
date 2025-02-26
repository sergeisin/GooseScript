using System;
using System.IO;
using System.Text;

namespace GooseScript
{
    internal static class XmlBuilder
    {
        public static void SaveSCL(GooseSettings settings, string iedName)
        {
            if (string.IsNullOrEmpty(iedName))
            {
                throw new FormatException("IED name can't be empty");
            }

            if (!settings.gocbRef.StartsWith(iedName))
            {
                throw new FormatException("GoCB reference must begin with IED name");            
            }

            if (!settings.datSet.StartsWith(iedName))
            {
                throw new FormatException("DataSet reference must begin with IED name");
            }

            string goRef = settings.gocbRef.Replace(iedName, "");
            string dsRef = settings.datSet.Replace(iedName, "");

            if (!TryParseRef(goRef, "/LLN0$GO$", out string ldName, out string goCbName))
            {
                throw new FormatException("Incorrect object reference 'gocbRef'");
            }

            if (!TryParseRef(dsRef, "/LLN0$", out string ldName2, out string dataSet))
            {
                throw new FormatException("Incorrect object reference 'datSet'");
            }

            if (ldName != ldName2)
            {
                throw new FormatException("Inconsistent LD name in 'gocbRef' and 'datSet'");
            }

            var sb = new StringBuilder(SCL_Template);

            if (settings.isStruct)
            {
                sb.Replace("_FCDA_", FCDA_DO);
            }
            else
            {
                sb.Replace("_FCDA_", settings.hasTimeStamp ? FCDA_DA3 : FCDA_DA2);
            }
            sb.Replace('\'', '"');

            string daType = string.Empty;
            switch (settings.mmsType)
            {
                case MMS_TYPE.INT32:   daType = "INT32";   break;
                case MMS_TYPE.INT32U:  daType = "INT32U";  break;
                case MMS_TYPE.FLOAT32: daType = "FLOAT32"; break;
                case MMS_TYPE.BOOLEAN: daType = "BOOLEAN"; break;

                // Next two types are not provided in SCL

                case MMS_TYPE.BIT_STRING:   daType = "Dbpos";   break;
                case MMS_TYPE.OCTET_STRING: daType = "Octet64"; break;

                // SCL type Dbpos mapped on MMS type BIT_STRING
                // SCL type Octet64 mapped on MMS type OCTET_STRING
            }
            
            var vlanID = settings.hasVlan ? settings.vlanID : 0;
            
            sb.Replace("_DA_TYPE_", daType);
            sb.Replace("_LD_NAME_", ldName);
            sb.Replace("_IED_NAME_", iedName);
            sb.Replace("_DATA_SET_", dataSet);
            sb.Replace("_GOCB_NAME_", goCbName);
            sb.Replace("_GOID_", settings.goId);
            sb.Replace("_CONF_REV_", settings.confRev.ToString());
            sb.Replace("_APP_ID_", settings.appID.ToString("X4"));
            sb.Replace("_VLAN_ID_", vlanID.ToString("X3"));
            sb.Replace("_DST_MAC_", settings.dstMac.ToString("X4").Insert(2, "-"));
            
            File.WriteAllText("GooseScript.cid", sb.ToString());
        }

        private static bool TryParseRef(string objRef, string sep, out string ldName, out string name)
        {
            ldName = string.Empty;
            name   = string.Empty;

            if (!objRef.Contains(sep))
                return false;

            string[] sArr = objRef.Split(new string[] { sep }, StringSplitOptions.RemoveEmptyEntries);
            if (sArr.Length != 2)
                return false;

            if (!char.IsLetter(sArr[0][0])) return false;
            if (!char.IsLetter(sArr[1][0])) return false;

            ldName = sArr[0];
            name   = sArr[1];
            return true;
        }

        #region SCL templates (do not format)

        private readonly static string FCDA_DO = @"
              <FCDA ldInst='_LD_NAME_' lnClass='LLN0' doName='Value' fc='ST' />";

        private readonly static string FCDA_DA2 = @"
              <FCDA ldInst='_LD_NAME_' lnClass='LLN0' doName='Value' fc='ST' daName='stVal' />
              <FCDA ldInst='_LD_NAME_' lnClass='LLN0' doName='Value' fc='ST' daName='q' />";

        private readonly static string FCDA_DA3 = @"
              <FCDA ldInst='_LD_NAME_' lnClass='LLN0' doName='Value' fc='ST' daName='stVal' />
              <FCDA ldInst='_LD_NAME_' lnClass='LLN0' doName='Value' fc='ST' daName='q' />
              <FCDA ldInst='_LD_NAME_' lnClass='LLN0' doName='Value' fc='ST' daName='t' />";

        private readonly static string SCL_Template =
@"<?xml version='1.0' encoding='utf-8'?>
<SCL xmlns='http://www.iec.ch/61850/2003/SCL' version='2007' revision='B'>
  <Header id='GooseScript' version='1.2' />
  <Communication>
    <SubNetwork name='SN'>
      <ConnectedAP iedName='_IED_NAME_' apName='AP'>
        <GSE ldInst='_LD_NAME_' cbName='_GOCB_NAME_'>
          <Address>
            <P type='MAC-Address'>01-0C-CD-01-_DST_MAC_</P>
            <P type='APPID'>_APP_ID_</P>
            <P type='VLAN-ID'>_VLAN_ID_</P>
            <P type='VLAN-PRIORITY'>4</P>
          </Address>
          <MinTime unit='s'>100</MinTime>
          <MaxTime unit='s'>1000</MaxTime>
        </GSE>
      </ConnectedAP>
    </SubNetwork>
  </Communication>
  <IED name='_IED_NAME_' manufacturer='github:sergeisin' originalSclVersion='2007' originalSclRevision='B'>
    <AccessPoint name='AP'>
      <Server>
        <LDevice inst='_LD_NAME_'>
          <LN0 lnType='LN_Type' lnClass='LLN0' inst=''>
            <DataSet name='_DATA_SET_'>_FCDA_
            </DataSet>
            <GSEControl type='GOOSE' name='_GOCB_NAME_' datSet='_DATA_SET_' confRev='_CONF_REV_' appID='_GOID_' />
          </LN0>
        </LDevice>
      </Server>
    </AccessPoint>
  </IED>
  <DataTypeTemplates>
    <LNodeType id='LN_Type' lnClass='LLN0'>
      <DO type='DO_Type' name='Value' />
    </LNodeType>
    <DOType id='DO_Type' cdc='SPS'>
      <DA name='stVal' fc='ST' bType='_DA_TYPE_' />
      <DA name='q' fc='ST' bType='Quality' />
      <DA name='t' fc='ST' bType='Timestamp' />
    </DOType>
  </DataTypeTemplates>
</SCL>
";

        #endregion
    }
}
