using Serilog;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace UmbrellaProject.Services
{
    public class UniqueProvider
    {
        private string GetHash(string s)
        {
            using (var sec = new MD5CryptoServiceProvider())
            {
                byte[] bt = Encoding.ASCII.GetBytes(s);
                return GetHexString(sec.ComputeHash(bt));
            }
        }

        private string GetHexString(byte[] bt)
        {
            string s = string.Empty;
            for (int i = 0; i < bt.Length; i++)
            {
                byte b = bt[i];
                int n = b;
                int n1 = n & 15;
                int n2 = (n >> 4) & 15;
                if (n2 > 9)
                    s += ((char)(n2 - 10 + 'A')).ToString(CultureInfo.InvariantCulture);
                else
                    s += n2.ToString(CultureInfo.InvariantCulture);
                if (n1 > 9)
                    s += ((char)(n1 - 10 + 'A')).ToString(CultureInfo.InvariantCulture);
                else
                    s += n1.ToString(CultureInfo.InvariantCulture);
                if ((i + 1) != bt.Length && (i + 1) % 2 == 0) s += "-";
            }
            return s;
        }

        private string _fingerPrint = string.Empty;
        public string Value()
        {
            //You don't need to generate the HWID again if it has already been generated. This is better for performance
            //Also, your HWID generally doesn't change when your computer is turned on but it can happen.
            //It's up to you if you want to keep generating a HWID or not if the function is called.
            if (string.IsNullOrEmpty(_fingerPrint))
            {
                _fingerPrint = GetHash("CPU >> " + CpuId() + "BIOS >> " + BiosId() + "BASE >> " + BaseId());// + "DISK >> " + DiskId());
            }
            return _fingerPrint;
        }

        //private string Identifier(string wmiClass, string wmiProperty, string wmiMustBeTrue)
        //{
        //    string result = "";
        //    System.Management.ManagementClass mc = new System.Management.ManagementClass(wmiClass);
        //    System.Management.ManagementObjectCollection moc = mc.GetInstances();
        //    foreach (System.Management.ManagementBaseObject mo in moc)
        //    {
        //        if (mo[wmiMustBeTrue].ToString() != "True") continue;
        //        //Only get the first one
        //        if (result != "") continue;
        //        try
        //        {
        //            result = mo[wmiProperty].ToString();
        //            break;
        //        }
        //        catch
        //        {
        //        }
        //    }
        //    return result;
        //}
        //Return a hardware identifier
        private string Identifier(string wmiClass, string wmiProperty)
        {
            string result = "";
            try
            {
                System.Management.ManagementClass mc = new System.Management.ManagementClass(wmiClass);
                System.Management.ManagementObjectCollection moc = mc.GetInstances();
                foreach (System.Management.ManagementBaseObject mo in moc)
                {
                    //Only get the first one
                    if (result != "") continue;
                    try
                    {
                        result = mo[wmiProperty].ToString();
                        Log.Error($"{wmiClass} {wmiProperty}: {result}", "HWID");
                        break;
                    }
                    catch
                    {
                        continue;
                    }
                }
            }
            catch
            {
            }
            return result;
        }

        private string CpuId()
        {
            var retVal = Identifier("Win32_Processor", "ProcessorId");
            if (retVal == "")
            {
                retVal = Identifier("Win32_Processor", "Manufacturer");
            }
            return retVal;
        }

        //BIOS Identifier
        private string BiosId()
        {
            return Identifier("Win32_BIOS", "Manufacturer") + Identifier("Win32_BIOS", "SMBIOSBIOSVersion") + Identifier("Win32_BIOS", "ReleaseDate") + Identifier("Win32_BIOS", "Version");
        }

        //Main physical hard drive ID
        private string DiskId()
        {
            return Identifier("Win32_DiskDrive", "Model") + Identifier("Win32_DiskDrive", "Manufacturer") + Identifier("Win32_DiskDrive", "Signature") + Identifier("Win32_DiskDrive", "TotalHeads");
        }

        //Motherboard ID
        private string BaseId()
        {
            return Identifier("Win32_BaseBoard", "Manufacturer") + Identifier("Win32_BaseBoard", "Name") + Identifier("Win32_BaseBoard", "SerialNumber");
        }
    }
}
