### Example with MMS type BIT_STRING (IEC type Dbpos)

```C#
{
    var publisher = new GoosePublisher(new GooseSettings()
    {
        interfaceName = "Ethernet",

        dstMac = 0x00E2,
        appID  = 0xBEEF,

        gocbRef = "IED1SYS/LLN0$GO$GSE1",
        datSet  = "IED1SYS/LLN0$DataSet",
        goID    = "IED1SYS/LLN0.GSE1",

        vlanID  = 0x005,
        hasVlan = true,

        mmsType = MMS_TYPE.BIT_STRING,
        hasTimeStamp = true
    });

    publisher.SaveSCL("IED1");

    publisher.Value = Dbpos.BadState;
    publisher.SendFew(5, 200);

    publisher.Value = Dbpos.Off;
    publisher.SendFew(5, 200);

    publisher.Value = Dbpos.Intermediate;
    publisher.SendFew(5, 200);

    publisher.Value = Dbpos.On;
    publisher.SendFew(5, 200);
}
```