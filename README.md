# GooseScript
Программа для публикации GOOSE сообщений в соответствии со стандартом IEC61850-8-1.  

## Возможности
- Низкоуровневый доступ к полям GOOSE-сообщения
- Произвольный выбор типа и структуры данных для публикации
- Управление при помощи редактора скриптов на языке C#
- Неограниченное количество сценариев использования
- Механизм ретрансляции GOOSE соответствует КП ФСК
- Возможность получения cid файла для подписки на публикуемый Goose

## Требования
- Windows 10 / 11
- Установленный .NET Framework 4.8
- Установленный драйвер Npcap 1.7

Драйвер Npcap распространяется с последней версией WireShark

## Примеры
Код из любого примера можно копировать в редактор скриптов и запустить

- [Описание всех подддерживаемых функций               ](Examples/Manual.txt)
- [Пример работы с типом Boolean                       ](Examples/Script_Boolean.txt)
- [Пример работы с численными типами Int и Float       ](Examples/Script_Numeric.txt)
- [Пример работы с типом Dbpos (положение выключателя) ](Examples/Script_Dbpos.txt)
- [Пример работы с типом Octet64                       ](Examples/Script_Octets.txt)
- [Пример работы с типом Quality                       ](Examples/Script_Quality.txt)

## Как пользоваться
В редакторе кода требуется набрать код на языке C#, используя преопределённые классы для работы с сетью.

```
// Объект для хранения настроек публикатора
var settings = new GooseSettings()
{
    interfaceName = "Ethernet X",

    gocbRef = "IED1SYS/LLN0$GO$GSE1",
    datSet  = "IED1SYS/LLN0$DataSet",
    goId    = "MyGoose"
};

//
var publisher = new GoosePublisher(settings);
publisher.Send();
```

Объект класса GooseSettings содержит множество настроек, большенство из которых являются опциональными.
Все обязательные настройки приведены в коде выше.
Полное описание приведено в примере 'Manual'.

## Стркутура набора данных

### MMStypes
Программа публикует набор данных с фиксированной стуктурой  
Это либо DO либо плоский список DA (stVal + q + t).

### MMS types
|MMS_TYPE     |     Value    |
|:------------|:------------:|
|BOOLEAN      | true / false |
|BIT_STRING   | "011010"     |
|INT32        | -42          |
|INT32U       | 404          |
|FLOAT32      | 3.14         |
|OCTET_STRING | "c0ffee"     |

## Редактор скриптов
- Ctrl + MouseWheel: изменение масштаба текста
- Ctrl + X: удаление текущей строки
- При закрытии окна весь код сохраняется в GooseScript.cs
- При удаленни данных из онка редактора после перезапуска будет восствновлен скрипт по умолчанию.



## Подписка на публикуемый Goose
Для подписки на публикуемое сообщейние есть два сценария
- Ручная подписка по данным, подсмотренным в WireShark
- Подписка с помошью cid файла и конфигуратора ICT

Сохранение cid-файла:
```
publisher.SaveSCL("IED1");
```
Важно отметить, что сохранение произойдёт только в том случае, если задаваемые вами значения
gocbRef и datSet более менее корректны:
- Ссылки начинаются с имени IED
- В качестве DO используется LLN0

## BOOLEAN
```
pub.Value = true;
...
pub.Value = !publisher.Value;
```

## INT32 - INT32U - FLOAT32
```
publisher.Value = 42;
...
publisher.Value++;
...
publisher.Value += 1.5;
```

## BIT_STRING
```
// Dbpos
pub.Value = Dbpos.Intermediate;
pub.Value = Dbpos.Off
pub.Value = Dbpos.On;
pub.Value = Dbpos.BadState;

// Raw
pub.Value =       "1_0101"; // bits:  5, padding: 3
pub.Value =      "01_0101"; // bits:  6, padding: 2
pub.Value =     "101_0101"; // bits:  7, padding: 1
pub.Value =    "0101_0101"; // bits:  8, padding: 0
pub.Value =  "1_0101_0101"; // bits:  9, padding: 7
pub.Value = "01_0101_0101"; // bits: 10, padding: 6
```

## OCTET_STRING
```
publisher.Value = "DEAD_BEEF";
.
publisher.Value += "BAAD_FOOD";
```

## Runtime operations with 'Quality' data attribute
```
publisher.Quality = new Quality()
{
    Validity = Validity.Invalid,
    Test = true,
    OperatorBlocked = true
};
```

## Send single message
```
publisher.Send();
```

## Send few messages with delay
```
publisher.SendFew(count, sleepTime);
```

## Retransmission mechanism
```
publisher.Run(minTime, maxTime);

while(true)
{
    // Some work with stVal
    Timer.Sleep(1000);
}
```

## Direct counters update (unsafe)
```
while(loop)
{
    pub.Send();
    pub.StNum = my_stNum;
    pub.SqNum = my_sqNum;
    pub.TAL   = my_TAL_ms;
}
```

## Scripting

```
pub.mmsType = MMS_TYPE.OCTET_STRING
pub.Value = "c0ffee"
```

## Run publishing
```
publisher.Run(minTime: 10, maxTime: 1000);
```

DataAttribute - stVal   (MMS_TYPE)
DataAttribute - q       (Quality)

BOOLEAN / BIT_STRING / INT32 / INT32U / FLOAT32 / OCTET_STRING

## Самостоятельная сборка
Для сборки можно использовать Microsoft Visual Studio Community

* Клоинровать проект github.com/sergeisin/GooseScript
* Обновить зависимости NuGet (SharpPcap) 
* Собрать Release
* Для получения одного исполняемого файла можно использовать ilMerge

```
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
