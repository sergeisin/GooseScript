# GooseScript
Программа для публикации GOOSE сообщений в соответствии со стандартом IEC61850-8-1.  

![](Screenshot.png)

## Возможности
- Управление при помощи скрипта на языке C#
- Доступ к любым полям Goose-сообщения
- Выбор типа и структуры данных для публикации
- Ручной или автоматический режим публикации
- Неограниченное количество сценариев использования
- Сохранение cid-файла для подписки на публикуемый Goose

## Требования
- Windows 10 / 11
- [Microsoft .NET Framework 4.8](https://dotnet.microsoft.com/ru-ru/download/dotnet-framework/net48)
- [Сетевой драйвер Npcap       ](https://npcap.com/#download)

> [!NOTE]
> Драйвер Npcap также устанавливается, при установке последней версии Wireshark

## Как пользоваться
- Набрать код на языке C#, используя предопределённые классы для работы с Goose.
- При нажатии на кнопку "Run Script" код будет скомпилирован и выполнен.
- В коде допустимо использование большинства языковых конструкций C# (переменные, циклы, операции и т.д.).
- В случае возникновения ошибки будет выведено соответствующее информативное сообщение.

> [!TIP]
> За основу скрипта можно взять один из примеров, поправив его под свои задачи.

## Примеры
Код из любого примера можно копировать в редактор скриптов и запустить

- [Описание всех подддерживаемых функций               ](Examples/Manual.md)
- [Пример работы с типом Boolean                       ](Examples/Script_Boolean.md)
- [Пример работы с численными типами Int и Float       ](Examples/Script_Numeric.md)
- [Пример работы с типом Dbpos (положение выключателя) ](Examples/Script_Dbpos.md)
- [Пример работы с типом Octet64                       ](Examples/Script_Octets.md)
- [Пример работы с типом Quality                       ](Examples/Script_Quality.md)
- [Публикация нескольких Goose одновременно            ](Examples/Script_TwoPublishers.md)

## Основные объекты
Для конфигурирования параметров Goose-сообщения потребуется объект класса ```GooseSettings```.  
Данный объект содержит множество настроек, большинство из которых являются опциональными.  
Все обязательные настройки приведены в коде ниже.

```C#
var settings = new GooseSettings()                  // Объект для хранения настроек публикатора
{
    interface = "Ethernet",

    gocbRef = "IED1SYS/LLN0$GO$GSE1",
    datSet  = "IED1SYS/LLN0$DataSet",
    goId    = "MyGooseID"
};

var publisher = new GoosePublisher(settings);       // Объект для управления публикацией
```

Для публикации Goose-сообщений потребуется объект класса ```GoosePublisher```,
который отвечает за работу с сетью.  
В конструктор объекта ```GoosePublisher``` следует передать объект типа ```GooseSettings```.

## Стркутура набора данных
Публикуемый набор данных может содержать список атрибутов данных (DA) или объект данных (DO) целиком.  
Стуктура набора данных задаётся на этапе создания конфигурации при помощи параметров ```isStruct``` и ```hasTimeStamp```.

Структура набора данных при ```settings.isStruct = true```
```
DataSet
{
    DataObject                      DO типа SPS / INS / DPS
    {
        DataAttribute - stVal       DA типа MMS_TYPE
        DataAttribute - q           DA типа Quality
        DataAttribute - t           DA типа Timestamp
    }
}
```

Структура набора данных при ```settings.isStruct = false``` и ```settings.hasTimeStamp = true```
```
DataSet
{
    DataAttribute - stVal           DA типа MMS_TYPE
    DataAttribute - q               DA типа Quality
    DataAttribute - t               DA типа Timestamp
}
```

Структура набора данных при ```settings.isStruct = false``` и ```settings.hasTimeStamp = false```
```
DataSet
{
    DataAttribute - stVal           DA типа MMS_TYPE
    DataAttribute - q               DA типа Quality
}
```

## Тип и значение stVal

Тип данных и начальное значение для stVal задаётся на этапе конфигурирования при помощи
параметров ```mmsType``` и ```initVal```.

```C#
settings.mmsType = MMS_TYPE.INT32;
settings.initVal = 42;
```

При присваивании ```initVal``` или ```publisher.Value``` тип выражения должен соответствовать типу данных stVal.

```C#
settings.mmsType = MMS_TYPE.INT32;

settings.initVal = false;   // Ошибка 'MMS type mismatch'
                            // Значение 'false' типа bool не конвертируется в int
```

Все поддерживаемые типы данных приведены в таблице

|MMS_TYPE     |Тип C#        | Пример   |
|:------------|:-------------|:--------:|
|BOOLEAN      |bool          | false    |
|INT32        |int, uint     | -42      |
|INT32U       |int, uint     | 404      |
|FLOAT32      |float, double | 3.14     |
|BIT_STRING   |string (bin)  | "011010" |
|OCTET_STRING |string (hex)  | "c0ffee" |

## Публикация сообщений
Управление публикацией осуществляется с помошью объекта ```GoosePublisher```. 
Предусмотрено два режима публикации ручной и автоматический.

В ручном режиме публикации:
- Требуется вручную вызывать метод ```.Send()```
- Требуется вручную устанавливать время жизни сообщений (TAL)
- Требуется вручную задавать интервалы времени между публикациями сообщений
- Изменение в наборе данных не приводит к отправке сообщений

```C#
var publisher = new GoosePublisher(settings);
...
publisher.TAL = 1500;           // Установка TimeAllowedToLive - 1500 мс
...
publisher.Send();               // Публикация одного сообщения
...
Timer.Sleep(500);               // Ожидание 500 мс
...
publisher.SendFew(5, 200);      // Публикация 5-и сообщений с интервалом 200 мс
```

Автоматический режим публикации запускается вызовом метода ```.Run(minTime, maxTime)```.  
В качестве параметров передаются минимальный и максимальный интервалы ретрансляции (в мс).

В автоматическом режиме публикации:
- Время жизни сообщений, а также время отправки определяются автоматически.
- Изменение ```.Value``` или ```.Quality``` в наборе данных приводит к мгновенной отправке сообщения и
умельшению интервала между сообщениями до минимального. 

Параметры автоматического режима ретрансляции сообщений соответствуют КП ФСК.

```C#
publisher.Run(10, 1000);        // Запуск автоматического механизма публикации

while(true)                     // В цикле происходит увеличение значения
{                               // stVal раз в 5 секунд
    publisher.Value++;
    Timer.Sleep(5000);
}
```

## Изменение значения stVal
С помошью свойства ```publisher.Value``` можно в любой момент времени изменить значение stVal.  
Тип ```.Value``` определяется исходя из ```mmsType``` набора данных.  
Для ```.Value``` допустимы все операции, которые допустимы для переменной того-же типа.

```settings.mmsType = MMS_TYPE.BOOLEAN;```
```C#
publisher.Value = true;
...
publisher.Value = false;
...
publisher.Value = !publisher.Value;
```

```settings.mmsType = MMS_TYPE.INT32;```
```C#
publisher.Value = 42;
...
publisher.Value++;
...
publisher.Value -= 3.14;
```

```settings.mmsType = MMS_TYPE.BIT_STRING;```
```C#
// Dbpos - тип данных для отображения состояния коммутационного аппарата

publisher.Value = Dbpos.Intermediate;   // "00"
publisher.Value = Dbpos.Off             // "01"
publisher.Value = Dbpos.On;             // "10"
publisher.Value = Dbpos.BadState;       // "11"

// Обобщенный тип bit string

publisher.Value =       "1_0101";   // bits:  5, padding: 3
publisher.Value =      "01_0101";   // bits:  6, padding: 2
publisher.Value =     "101_0101";   // bits:  7, padding: 1
publisher.Value =    "0101_0101";   // bits:  8, padding: 0
publisher.Value =  "1_0101_0101";   // bits:  9, padding: 7
publisher.Value = "01_0101_0101";   // bits: 10, padding: 6
```

```settings.mmsType = MMS_TYPE.OCTET_STRING;```
```C#
publisher.Value = "DEAD_BEEF";
.
publisher.Value += "BAAD_FOOD";
```

## Изменение качества данных
Для изменения качества данных требуется свойству ```publisher.Quality```
установить новое значение типа ```Quality```.

```C#
var q = new Quality()
{
    Validity        = Validity.Good,    // .Invalid | .Reserved | .Questionable
    Overflow        = true,
    OutofRange      = true,
    BadReference    = true,
    Oscillatory     = true,
    Failure         = true,
    OldData         = true,
    Inconsistent    = true,
    Inaccurate      = true,
    Source          = true,
    Test            = true,
    OperatorBlocked = true
};

publisher.Quality = q;
...
publisher.Quality = new Quality()       // Способ без дополнительной переменной
{
    Validity = Validity.Questionable,
    Failure  = true,
    Test     = true
};
```

## Счётчики stNum и sqNum
При каждом изменении данных счётчик stNum увеличивается на 1, а счётчик sqNum сбрасывается в 0.
При каждой ретрансляции сообщения без изменения данных счётчик sqNum увеличивается на 1.  

Кроме того предоусмотрено прямое управление счётчиками. Логика увеличения счётчиков при этом не отключается.

```C#
publisher.StNum = 42;
publisher.SqNum = 43;
```

## Режим симуляции
-- ToDO

## Подписка на публикуемый Goose
Для подписки на публикуемое сообщение есть два сценария
- Ручная подписка по данным, подсмотренным в WireShark
- Подписка с помошью cid-файла и конфигуратора ICT

Сохранение cid-файла:
```C#
publisher.SaveSCL("IED1");
...
publisher.SaveSCL("IED1", "myFile.cid");
```
Важно отметить, что сохранение произойдёт только в том случае, если задаваемые вами значения
gocbRef и datSet более менее корректны:
- Ссылки начинаются с имени IED
- В качестве DO используется LLN0

## Редактор скриптов
- Ctrl + MouseWheel: изменение масштаба текста
- Ctrl + X: удаление текущей строки
- При закрытии окна весь код сохраняется в GooseScript.cs
- При удаленни данных из онка редактора после перезапуска будет восствновлен скрипт по умолчанию.
 
## Самостоятельная сборка
Для сборки можно использовать Microsoft Visual Studio Community

* Клоинровать проект github.com/sergeisin/GooseScript
* Обновить зависимости NuGet (SharpPcap) 
* Собрать Release
* Для получения одного исполняемого файла можно использовать ilMerge

```Batchfile
.\ILMerge /target:winexe /out:Out.exe ^
GooseScript.exe ^
PacketDotNet.dll ^
SharpPcap.dll ^
System.Buffers.dll ^
System.Memory.dll ^
System.Numerics.Vectors.dll ^
System.Runtime.CompilerServices.Unsafe.dll ^
System.Text.Encoding.CodePages.dll

del /q GooseScript.exe
del /q Out.pdb

ren Out.exe GooseScript.exe
```

## Ограничения
- Нельзя увеличить набор данных. Например, добавить несколько DO.
- Файл конфигурации (.cid) лишь примерно соответствует схеме SCL и не подлежим валидации.

## Автор
- [@Sergey Sinitcyn](https://github.com/sergeisin)
- <u>sergei28.01.1994@gmail.com</u>

## Применение
Программа может быть полезна разработчикам и наладчикам устройств РЗА,
а так-же другим специалистам, занимающимся отладкой приёма и публикации GOOSE-сообщейни.

## Сценарии применения
- Проверка приёма сообщений с различной стукрутой данных
- Проверка возможности приёма различных типов данных (BOOLEAN / INT32 / Dbpos)
- Проведение ping-pong тестирования производительности Goose
- Проверка возможности приёма сообщейний с разными значениями VlanID
- Проверка работы IED при переполнении счётчиков stNum или sqNum
- Проверка работы IED при приёме симулированных Goose-сообщений
- Проверка соответствия устройства и документации (PIXIT)
