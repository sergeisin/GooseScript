### Example with MMS type BOOLEAN

```C#
{
    var publisher = new GoosePublisher(new GooseSettings()
    {
        interfaceName = "Ethernet",

        dstMac = 0x00E1,
        appID  = 0xDEAD,

        gocbRef = "IED1SYS/LLN0$GO$GSE1",
        datSet  = "IED1SYS/LLN0$DataSet",
        goID    = "IED1SYS/LLN0.GSE1",

        mmsType = MMS_TYPE.BOOLEAN,
        initVal = true
    });

    publisher.SaveSCL("IED1", "Example");
    
    publisher.Run(100, 1000);

    while (true)
    {
        Timer.Sleep(2500);
        publisher.Value = false;

        Timer.Sleep(2500);
        publisher.Value = !publisher.Value;
    }
}
```