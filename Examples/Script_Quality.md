### Example with Quality change

```C#
{
    var publisher = new GoosePublisher(new GooseSettings()
    {
        interfaceName = "Ethernet",

        dstMac = 0x00E5,
        appID  = 0xF1FA,

        gocbRef = "IED1SYS/LLN0$GO$GSE1",
        datSet  = "IED1SYS/LLN0$DataSet",
        goID    = "IED1SYS/LLN0.GSE1",

        mmsType = MMS_TYPE.BOOLEAN
    });

    publisher.SendFew(5, 500);

    publisher.Quality = new Quality()
    {
        OperatorBlocked = true,
        Test = true
    };

    publisher.SendFew(5, 500);

    publisher.Quality = new Quality()
    {
        Validity = Validity.Invalid
    };

    publisher.SendFew(5, 500);
}
```