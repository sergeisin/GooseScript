### Example with MMS type OCTET_STRING

```C#
{
    var publisher = new GoosePublisher(new GooseSettings()
    {
        interfaceName = "Ethernet",

        dstMac = 0x00E4,
        appID  = 0xBABE,

        gocbRef = "IED1SYS/LLN0$GO$GSE1",
        datSet  = "IED1SYS/LLN0$DataSet",
        goID    = "IED1SYS/LLN0.GSE1",

        mmsType = MMS_TYPE.OCTET_STRING,
        initVal = "1234_6789_ABCD_EF00"
    });

    publisher.Run(100, 1000);

    for (int i = 0; i < 10; i++)
    {
        publisher.Value += "c0ffee";

        Timer.Sleep(2500);
    }
}
```