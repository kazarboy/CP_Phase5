using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace EPP.CorporatePortal.Models
{
    /// <summary>
    /// Provides access to a network share.
    /// </summary>
    public class NetworkShareAccesser : IDisposable
    {
        private string _remoteUncName;
        private string _remoteComputerName;

        public string RemoteComputerName
        {
            get
            {
                return this._remoteComputerName;
            }
            set
            {
                this._remoteComputerName = value;
                this._remoteUncName = this._remoteComputerName;
            }
        }

        public string UserName
        {
            get;
            set;
        }
        public string Password
        {
            get;
            set;
        }

        #region ConstsReferences

        //RESOURCE_CONNECTED = 0x00000001;
        //RESOURCE_GLOBALNET = 0x00000002;
        //RESOURCE_REMEMBERED = 0x00000003;

        //RESOURCETYPE_ANY = 0x00000000;
        //RESOURCETYPE_DISK = 0x00000001;
        //RESOURCETYPE_PRINT = 0x00000002;

        //RESOURCEDISPLAYTYPE_GENERIC = 0x00000000;
        //RESOURCEDISPLAYTYPE_DOMAIN = 0x00000001;
        //RESOURCEDISPLAYTYPE_SERVER = 0x00000002;
        //RESOURCEDISPLAYTYPE_SHARE = 0x00000003;
        //RESOURCEDISPLAYTYPE_FILE = 0x00000004;
        //RESOURCEDISPLAYTYPE_GROUP = 0x00000005;

        //RESOURCEUSAGE_CONNECTABLE = 0x00000001;
        //RESOURCEUSAGE_CONTAINER = 0x00000002;


        //CONNECT_INTERACTIVE = 0x00000008;
        //CONNECT_PROMPT = 0x00000010;
        //CONNECT_REDIRECT = 0x00000080;
        //CONNECT_UPDATE_PROFILE = 0x00000001;
        //CONNECT_COMMANDLINE = 0x00000800;
        //CONNECT_CMD_SAVECRED = 0x00001000;

        //CONNECT_LOCALDRIVE = 0x00000100;

        #endregion

        #region ErrorsReferences

        //NO_ERROR = 0;

        //ERROR_ACCESS_DENIED = 5;
        //ERROR_ALREADY_ASSIGNED = 85;
        //ERROR_BAD_DEVICE = 1200;
        //ERROR_BAD_NET_NAME = 67;
        //ERROR_BAD_PROVIDER = 1204;
        //ERROR_CANCELLED = 1223;
        //ERROR_EXTENDED_ERROR = 1208;
        //ERROR_INVALID_ADDRESS = 487;
        //ERROR_INVALID_PARAMETER = 87;
        //ERROR_INVALID_PASSWORD = 1216;
        //ERROR_MORE_DATA = 234;
        //ERROR_NO_MORE_ITEMS = 259;
        //ERROR_NO_NET_OR_BAD_PATH = 1203;
        //ERROR_NO_NETWORK = 1222;

        //ERROR_BAD_PROFILE = 1206;
        //ERROR_CANNOT_OPEN_PROFILE = 1205;
        //ERROR_DEVICE_IN_USE = 2404;
        //ERROR_NOT_CONNECTED = 2250;
        //ERROR_OPEN_FILES = 2401;

        #endregion

        #region Consts

        private const int RESOURCETYPE_DISK = 0x00000001;

        private const int CONNECT_INTERACTIVE = 0x00000008;
        private const int CONNECT_PROMPT = 0x00000010;
        private const int CONNECT_UPDATE_PROFILE = 0x00000001;

        #endregion

        #region Errors

        private const int NO_ERROR = 0;

        #endregion

        #region PInvoke Signatures

        [DllImport("Mpr.dll")]
        private static extern int WNetUseConnection(
            IntPtr hwndOwner,
            NETRESOURCE lpNetResource,
            string lpPassword,
            string lpUserID,
            int dwFlags,
            string lpAccessName,
            string lpBufferSize,
            string lpResult
            );

        [DllImport("Mpr.dll")]
        private static extern int WNetCancelConnection2(
            string lpName,
            int dwFlags,
            bool fForce
            );

        [StructLayout(LayoutKind.Sequential)]
        private class NETRESOURCE
        {
            public int dwScope = 0;
            public int dwType = 0;
            public int dwDisplayType = 0;
            public int dwUsage = 0;
            public string lpLocalName = "";
            public string lpRemoteName = "";
            public string lpComment = "";
            public string lpProvider = "";
        }

        #endregion

        /// <summary>
        /// Creates a NetworkShareAccesser for the given computer name. The user will be promted to enter credentials
        /// </summary>
        /// <param name="remoteComputerName"></param>
        /// <returns></returns>
        public static NetworkShareAccesser Access(string remoteComputerName)
        {
            return new NetworkShareAccesser(remoteComputerName);
        }

        /// <summary>
        /// Creates a NetworkShareAccesser for the given computer name using the given domain/computer name, username and password
        /// </summary>
        /// <param name="remoteComputerName"></param>
        /// <param name="domainOrComuterName"></param>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        public static NetworkShareAccesser Access(string remoteComputerName, string domainOrComuterName, string userName, string password)
        {
            return new NetworkShareAccesser(remoteComputerName,
                                            domainOrComuterName + @"\" + userName,
                                            password);
        }

        /// <summary>
        /// Creates a NetworkShareAccesser for the given computer name using the given username (format: domainOrComputername\Username) and password
        /// </summary>
        /// <param name="remoteComputerName"></param>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        public static NetworkShareAccesser Access(string remoteComputerName, string userName, string password)
        {
            return new NetworkShareAccesser(remoteComputerName,
                                            userName,
                                            password);
        }

        private NetworkShareAccesser(string remoteComputerName)
        {
            RemoteComputerName = remoteComputerName;

            this.ConnectToShare(this._remoteUncName, null, null, true);
        }

        private NetworkShareAccesser(string remoteComputerName, string userName, string password)
        {
            RemoteComputerName = remoteComputerName;
            UserName = userName;
            Password = password;

            this.ConnectToShare(this._remoteUncName, this.UserName, this.Password, false);
        }

        private void ConnectToShare(string remoteUnc, string username, string password, bool promptUser)
        {
            NETRESOURCE nr = new NETRESOURCE
            {
                dwType = RESOURCETYPE_DISK,
                lpRemoteName = remoteUnc
            };

            int result;
            if (promptUser)
            {
                result = WNetUseConnection(IntPtr.Zero, nr, "", "", CONNECT_INTERACTIVE | CONNECT_PROMPT, null, null, null);
            }
            else
            {
                result = WNetUseConnection(IntPtr.Zero, nr, password, username, 0, null, null, null);
            }

            if (result != NO_ERROR)
            {
                throw new Win32Exception(result);
            }
        }

        private void DisconnectFromShare(string remoteUnc)
        {
            int result = WNetCancelConnection2(remoteUnc, CONNECT_UPDATE_PROFILE, false);
            if (result != NO_ERROR)
            {
                throw new Win32Exception(result);
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            this.DisconnectFromShare(this._remoteUncName);
        }
    }
}