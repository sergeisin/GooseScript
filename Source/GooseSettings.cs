using System;

namespace GooseScript
{
    public class GooseSettings
    {
        /// <summary>
        /// Interface Name
        /// </summary>
        public string interfaceName;

        /// <summary>
        /// Destination MAC-address (last two octets in range 0x0000 .. 0x03FF)
        /// </summary>
        public ushort dstMac = 0x0001;

        /// <summary>
        /// Adding VLan tag to Goose frame
        /// </summary>
        public bool hasVlan;

        /// <summary>
        /// Virtual lan ID in range 0x000 - 0xFFF
        /// </summary>
        public ushort vlanID;

        /// <summary>
        /// Application Id
        /// </summary>
        public ushort appID;

        /// <summary>
        /// Reference to a GOOSE Control Block
        /// </summary>
        public string gocbRef;

        /// <summary>
        /// Reference to source Data Set
        /// </summary>
        public string datSet;

        /// <summary>
        /// Goose ID
        /// </summary>
        public string goId;

        /// <summary>
        /// Configuration revision
        /// </summary>
        public uint confRev;

        /// <summary>
        /// Simulation flag in Reserved1 section
        /// </summary>
        public bool simulation_reserved;

        /// <summary>
        /// Simulation flag in goosePdu section
        /// </summary>
        public bool simulation_goosePdu;

        /// <summary>
        /// ndsCom flag
        /// </summary>
        public bool ndsCom;

        /// <summary>
        /// Time allowed to live (ms)
        /// </summary>
        public uint TAL;

        internal void Validate()
        {
            if (goId.Length > 129)
            {
                throw new ArgumentException($"GoID max length is 129 characters");
            }

            // Not Implemented
        }
    }
}
