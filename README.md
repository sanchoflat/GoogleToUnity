
# Для чего? #

* Генерация xml и ScriptableObject на основе Google таблиц для Unity

# Как настроить? #

1. Создаём таблицу 
![1.png](https://bitbucket.org/repo/gLan95/images/3915615596-1.png)

2. В правом верхнем углу жмём кнопку **Share** и выставляем тип доступа к таблице 
![2.png](https://bitbucket.org/repo/gLan95/images/651845763-2.png)

3. В Unity переходим в **Tools->Google Sheet Integration** и жмём **Create Config** 
![4.png](https://bitbucket.org/repo/gLan95/images/890118217-4.png)

4. Заполняем все поля аналогичным образом и жмём кнопку сверху **Generate class files**. После того, как загрузился класс и Unity его скомпилировал, нажимаем **Generate data files**. 
![5.png](https://bitbucket.org/repo/gLan95/images/198756042-5.png)




# Поддерживаемые типы данных #

* bool
* int
* float
* string
* bool []
* int []
* float []
* string []

# Структура таблицы #


# Настройки ассета #

### Главное окно ###

* **Generate class files** - загружает все Google таблицы, описанные в **Google sheet data** и генерирует файлы .cs.

* Generate data files - загружает все Google таблицы, описанные в **Google sheet data** и генерирует файлы данных (SO или xml). Если в assembly нет нужного класса, то выводится LogWarning.

### Окно **Google sheet data** ###

Описываются все таблицы, из которых нужно грузить данные.

* **Sheet Name** - просто имя таблицы для удобства. Нигде не используется
* **GoogleDriveFileGuis** - 
* **GoogleDriveSheetGuid** - 
* **Data extension** - расширение data файла. Если это SO, то расширение устанавливается по дефолту .so.
* **Data type** - выбор типа генерируемого файла (SO, xml).
* **Namespace** - при генерации класса (.cs), он будет помещён в данный namespace.
* **Class location** - путь, по которому будет сохранён файл с классом.
* **Data Location** - путь, по которому будет сохранён файл с данными.
* **Variable type** - тип генерируемых переменных (свойство или поле). Доступно только для xml.
* **Field access modifier** - возможность контролировать права доступа для генерируемых переменных (public|protected|private).


### Окно **Settings** ###

* **Skip prefix** - если в первом столбце таблицы встречается запись, начинающаяся с данного префикса, то это запись идёт в класс в виде разделителя (генерируются атрибуты Space и Header).
* **Constant class name** - имя класса с константами (пока не доступно).
* **Constant class location** - местоположение класса с константами (пока не доступно).
* **Comment column title** - имя столбца, где находятся комментарии. Данный столбец содержит комментарии и генерирует Tooltip и xml комментарии в .cs файл. Допускается только **один** столбец с комментарием.
* **Array separator** - разделитель для задания массивов.






### Who do I talk to? ###