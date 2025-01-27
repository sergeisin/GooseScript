﻿using System;

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
        public ushort dstMac;

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

        public void Validate()
        {
            if (dstMac > 0x03FF)
            {
                throw new ArgumentException("The valid range for the 'dstMac' is from 0 to 3FF");
            }

            if (vlanID > 0x0FFF)
            {
                throw new ArgumentException("The valid range for the 'vlanID' is from 0 to FFF");
            }

            if (gocbRef is null)
            {
                throw new ArgumentException("The value 'gocbRef' cannot be null");
            }

            if (gocbRef.Length > 129)
            {
                throw new ArgumentException("'gocbRef' max length is 129 characters");
            }

            if (datSet is null)
            {
                throw new ArgumentException("The value 'datSet' cannot be null");
            }

            if (datSet.Length > 129)
            {
                throw new ArgumentException("'datSet' max length is 129 characters");
            }

            if (goId is null)
            {
                throw new ArgumentException("The value 'goId' cannot be null");
            }

            if (goId.Length > 129)
            {
                throw new ArgumentException("'goId' max length is 129 characters");
            }
        }
    }
}
