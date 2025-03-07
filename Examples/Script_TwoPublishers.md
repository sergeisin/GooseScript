### Example with two goose publishers

```C#
{
    var pub_1 = new GoosePublisher(new GooseSettings()
    {
        interfaceName = "Ethernet",
        gocbRef = "IED1_SYS/LLN0$GO$GSE1",
        datSet  = "IED1_SYS/LLN0$DataSet",
        goId    = "Goose 1",
        appID   = 0xB1BA
    });

    var pub_2 = new GoosePublisher(new GooseSettings()
    {
        interfaceName = "Ethernet",
        gocbRef = "IED2_SYS/LLN0$GO$GSE2",
        datSet  = "IED2_SYS/LLN0$DataSet",
        goId    = "Goose 2",
        appID   = 0xB0BA
    });

    pub_1.Run(100, 1000);
    pub_2.Run(100, 1000);

    bool myVar = true;
    while (true)
    {
        Timer.Sleep(2500); pub_1.Value = myVar;
        Timer.Sleep(2500); pub_2.Value = myVar;
        myVar = !myVar;
    }
}
```