# Возможности #

* Генерация xml и ScriptableObject на основе Google таблиц для Unity в один клик.
* Можно использовать графики, функции и формулы, доступные в Google Sheets.
* Таблицы позволяют хранить всю историю изменений, так что ваши данные не пропадут.

# Как настроить? #

1. Создаём таблицу 
![1.png](https://bitbucket.org/repo/gLan95/images/3915615596-1.png)

2. В правом верхнем углу жмём кнопку **Share** и выставляем тип доступа к таблице 
![2.png](https://bitbucket.org/repo/gLan95/images/651845763-2.png)

3. В Unity переходим в **Tools->Google Sheet Integration** и жмём **Create Config** 
![4.png](https://bitbucket.org/repo/gLan95/images/3689851442-4.png)

4. Заполняем все поля аналогичным образом и жмём кнопку сверху **Generate class files**. После того, как загрузился класс и Unity его скомпилировал, нажимаем **Generate data files**. 
![5.png](readme/PluginView.png)

# Поддерживаемые типы данных #

* *bool*
* *int*
* *long*
* *float*
* *string*
* *bool []*
* *int []*
* *long []*
* *float []*
* *string []*

# Автоматический вывод типа строки #

Тип строки выводится автоматически на основе данных, находящихся в строке.
Если все типы данных в строке совпадают, то общий тип будет соответствующим.
Если типы данных не совпадают, то тип выводится как более общий для всех. 

Например строка состоит из следующих типов:

* int int int float -> **float**. Результирующий тип строки - **float**
* bool string int -> **string**. Результирующий тип - **string**. 
* int int long -> **long**. Результирующий тип - **long**. 
* int float bool long string -> **string**. Результирующий тип - **string**. 

**Если в строке находится хотя бы одна ячейка типа string, то вся строка имеет тип string.**

**Числовые типы чувствительны к "." и ",".** К примеру значение **4,5** будет распознано как строка.

Если вы хотите, чтобы строка имела тип float, но значения являются целыми, необходимо принудительно указать таблице, чтобы она отображала значения с цифрами после запятой (*increase decimal places*).

# Структура URL #

URL состоит из двух частей - **id таблицы** и **id листа** в этой таблице.

![3.png](https://bitbucket.org/repo/gLan95/images/4070177516-3.png)
**id таблицы** вставляется в **GoogleDriveFileGuid**.
**id листа** вставляется в **GoogleDriveSheetGuid**.
Если вы делаете одну таблицу, которая включает много разных листов, то id таблицы везде одинаковый, а id листа уникален для каждого листа.


# Структура таблицы #

![1.png](https://bitbucket.org/repo/gLan95/images/801700501-1.png)

# Создание массивов #

Есть 2 способа задания массивов: путём создания каждого элемента массива на новой строчке и путём перечисления всех элементов через **разделитель**. Разделитель настраивается в поле **Array separator**.
Эти методы можно комбинировать. На примере ниже все эти записи дадут на выходе массив следующего вида: 
*int[] {10, 15, 30, 45, 65}*.

![6.png](https://bitbucket.org/repo/gLan95/images/1438483726-6.png)

# Настройки ассета #

***После изменения данных в конфиге - лучше всего сохранить конфиг. После перекомпиляции скриптов, Unity потеряет все несохранённые изменения***

### Главное окно ###

* **Generate class files** - Загружает все Google таблицы, описанные в **Google sheet data** и генерирует файлы .cs.

* **Generate data files** - Загружает все Google таблицы, описанные в **Google sheet data** и генерирует файлы данных (SO или xml). Если в assembly нет нужного класса, то выводится LogWarning.

### Окно **Google sheet data** ###

Описываются все таблицы, из которых нужно грузить данные.

* **Sheet Name** - Просто имя таблицы для удобства. Нигде не используется.
* **GoogleDriveFileGuid** - id таблицы.
* **GoogleDriveSheetGuid** - id листа в таблице.
* **Data extension** - Расширение data файла. Если это SO, то расширение устанавливается по дефолту .so.
* **Data type** - Выбор типа генерируемого файла (SO, xml).
* **Namespace** - При генерации класса (.cs), он будет помещён в данный namespace.
* **Class location** - Путь, по которому будет сохранён файл с классом.
* **Data Location** - Путь, по которому будет сохранён файл с данными.
* **Variable type** - Тип генерируемых переменных (свойство или поле). Доступно только для xml.
* **Field access modifier** - Возможность контролировать права доступа для генерируемых переменных (public|protected|private).
* **Generate GET method** - Если флаг установлен, то генерируются методы, которые возвращают содиржимое ассета по типам (***int, int[], float, float[]...***).
* **Get method type** - Есть два варианта: ***string*** и ***enum***.
***string***: Создает методы, где в качестве ключа выступает строка (имя переменной);
***enum***: создается enum с именем ***Enum type name***, который дублирует все имена переменных. Все ***Get*** методы принимают в себя этот ***enum***.
* **Enum type name** - Имя перечисления.
* **Generate class file** - Генерирует файл класса только для данного листа.
* **Generate *data* file** - Генерирует файл с данными для данного листа.
* **Remove** - Удаляет данный sheet data из конфига.
* **+** - Добавляет ещё один **sheet data** .
* **-** - Удаляет последний **sheet data**.

### Окно **Settings** ###

* **Skip prefix** - Если в первом столбце таблицы встречается запись, начинающаяся с данного префикса, то это запись идёт в класс в виде разделителя (генерируются атрибуты Space и Header).
* **Use type name** - Использовать ли столбец с типами для каждой строки или позволить программе автоматически его вычислять (пока не используется).
* **Type column name** - Имя колонки, в которой определен тип строки (пока не используется).
* **Constant class name** - Имя класса с константами (пока не используется).
* **Constant class location** - Местоположение класса с константами (пока не используется).
* **Comment column title** - Имя столбца, где находятся комментарии. Данный столбец содержит комментарии и генерирует Tooltip и xml комментарии в .cs файл. Допускается только **один** столбец с комментарием.
* **Array separator** - разделитель для задания массивов.
* **Load config** - загружает сохранённый конфиг из папки.
* **Save current config** - сохраняет все изменённые значения в файл.