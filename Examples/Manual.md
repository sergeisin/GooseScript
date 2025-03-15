###  This example shows all the functionality

```C#
{
    var sets = new GooseSettings();             // All settings

    // L2 settings
    sets.interfaceName = "Ethernet";
    sets.dstMac = 0x01FF;                       // 0 .. 0x03FF (01-0C-CD-01-XX-XX)
    sets.appID  = 0xDEAD;                       // 0 .. 0xFFFF
    sets.vlanID = 0x005;                        // 0 .. 0xFFF
    sets.hasVlan = true;                        // Add VLan tag

    // GoosePDU settings
    sets.gocbRef = "IED1SYS/LLN0$GO$GSE1";      // MMS-notation GoCB reference
    sets.datSet  = "IED1SYS/LLN0$DataSet";      // MMS-notation DataSet reference
    sets.goId    = "IED1SYS/LLN0.GSE1";         // Goose ID

    sets.TAL     =  2000;                       // Time allowed ti live (ms)
    sets.confRev = 10000;                       // Configuration revision

    sets.simulation_reserved = false;           // Simulation flag in Reserved 1
    sets.simulation_goosePdu = false;           // Simulation flag in GoosePDU
    sets.ndsCom = false;                        // NdsCom flag

    // DataSet settings
    sets.mmsType = MMS_TYPE.INT32;              // MMS type of stVal DA
    sets.initVal = -10;                         // Initial value of stVal DA

    sets.isStruct = true;                       // DO struct { stVal + q + t }
    sets.isStruct = false;                      // DA struct   stVal + q
    sets.hasTimeStamp = true;                   // DA struct   stVal + q + t

    var pub = new GoosePublisher(sets);         // Publisher object

    // Save .cid file
    pub.SaveSCL("IED1");                        // Arg 1 - iedName 
    pub.SaveSCL("IED1", "MyFile.cid");          // Arg 2 - fileName (optional)

    // Manual sending
    pub.Send();                                 // Send single message
    pub.SendFew(count: 5, sleepTime: 400);      // Send 5 messages with 400 ms delay

    // Automatic sending
    pub.Run(minTime: 100, maxTime: 1000);       // Start retransmission

    Timer.Sleep(2500);                          // High precision sleep(ms)

    pub.Value++;                                // Update stVal
    Timer.Sleep(2500);

    pub.Quality = new Quality()                 // Update q
    {
        Test = true
    };

    Timer.Sleep(2500);
    pub.Stop();                                 // Stop retransmission

    // Direct control
    pub.TAL = 1000;
    pub.StNum = 42;
    pub.SqNum = 43;

    pub.Simulation = true;                      // Set both simulation flags to true
    pub.Send();
    
    // Quality features
    Quality q = new Quality();

    q.Validity = Validity.Good;
    q.Validity = Validity.Invalid;
    q.Validity = Validity.Questionable;

    q.Overflow        = true;
    q.OutofRange      = true;
    q.BadReference    = true;
    q.Oscillatory     = true;
    q.Failure         = true;
    q.OldData         = true;
    q.Inconsistent    = true;
    q.Inaccurate      = true;
    q.Source          = true;
    q.Test            = true;
    q.OperatorBlocked = true;

    pub.Quality = q;
    
    pub.Send();
}
```