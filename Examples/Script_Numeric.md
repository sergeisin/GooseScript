### Example with numeric MMS types INT32 / INT32U / FLOAT32

```C#
{
    var publisher = new GoosePublisher(new GooseSettings()
    {
        interfaceName = "Ethernet",

        dstMac = 0x00E3,
        appID  = 0xCAFE,

        gocbRef = "IED1SYS/LLN0$GO$GSE1",
        datSet  = "IED1SYS/LLN0$DataSet",
        goID    = "IED1SYS/LLN0.GSE1",

        mmsType = MMS_TYPE.INT32,
        initVal = -10,

        isStruct = true
    });

    publisher.Run(100, 1000);

    for (int i = 0; i < 5; i++)
    {
        Timer.Sleep(2500);
        publisher.Value = 32;

        Timer.Sleep(2500);
        publisher.Value += 10;
    }
}
```