# Лабораторная работа №1 
## Вариант 1

### Сборка Nuget пакета
1. Скачать [нейронную сеть ArcFace](https://github.com/onnx/models/blob/main/vision/body_analysis/arcface/model/arcfaceresnet100-8.onnx).
2. Скачать папку `ArcFaceNuget` из текущего репозитория. 
3. Выполнить в папке `ArcFaceNuget` команду 
```
dotnet build
```
Пример вывода работы команды:
```
PS C:\Users\olind\Desktop\401_indychko-master\ArcFaceNuget> dotnet build
MSBuild version 17.3.1+2badb37d1 for .NET
  Определение проектов для восстановления...
  Восстановлен C:\Users\olind\Desktop\401_indychko-master\ArcFaceNuget\ArcFaceNuget.csproj (за 140 ms).
  ArcFaceNuget -> C:\Users\olind\Desktop\401_indychko-master\ArcFaceNuget\bin\Debug\net6.0\ArcFaceNuget.dll
  Пакет "C:\Users\olind\Desktop\401_indychko-master\ArcFaceNuget\bin\Debug\ArcFaceNuget.1.0.0.nupkg" успешно создан.

Сборка успешно завершена.
    Предупреждений: 0
    Ошибок: 0

Прошло времени 00:00:09.68
```
4. Из вывода терминала взять путь до пакета (в примере `C:\Users\olind\Desktop\401_indychko-master\ArcFaceNuget\bin\Debug\ArcFaceNuget.1.0.0.nupkg`) и в папке с проектом, где должен использоваться компонент, выполнить команды
```
dotnet nuget add source --name ArcFaceNuget C:\Users\olind\Desktop\401_indychko-master\ArcFaceNuget\bin\Debug\ArcFaceNuget.1.0.0.nupkg

dotnet add package ArcFaceNuget
```
5. В проекте использовать API с помощью `using ArcFaceNuget;`

### Работа тестового приложения `TestNugetPackage`
После подключения пакета ArcFaceNuget программа вычисляет сходства и различия между двумя тестовыми изображениями. Результат выводится в консоль в виде двух матриц Distance и Similarity:
```
Distance Matrix
0 0,68851733
0,68851733 0

Similarity matrix
1 0,76297194
0,76297194 1
```

### Предоставляемый метод для работы с изображениями
```  
/// <summary>
/// Method gets N images and calculates distance and similarity between every two images.
/// </summary>
/// <returns>
/// Tuple with 2 matrix of size N x N. First matrix is distance matrix and another is similarity matrix.
/// </returns>
Task<(float[,], float[,])> GetDistanceAndSimilarity(Image<Rgb24>[] images, CancellationToken token)
```

# Лабораторная работа №2 
## Вариант 1Б

### Пример работы приложения
![WPF example](https://i.postimg.cc/Wb7yqysB/2022-10-31-035232.jpg)

# Лабораторная работа №3

### Пример работы приложения
![WPF with DB example](https://i.postimg.cc/Y283SWgG/image.png)

# Лабораторная работа №4

### Выставленное API для работы с базой данных и использования нейросети ArcFace.
![WPF Web Api](https://i.postimg.cc/J7j77yqC/api.jpg)

### Пример логирования сервера действий пользователя
```
info: WPFArcFaceApi.Controllers.ArcFaceController[0]
      User gets all images ids from database.
info: WPFArcFaceApi.Controllers.ArcFaceController[0]
      GetImagesList result contains 2 images with IDs: 13 14
info: WPFArcFaceApi.Controllers.ArcFaceController[0]
      User gets image info by id = 13 from database.
info: WPFArcFaceApi.Controllers.ArcFaceController[0]
      For image with id = 13 find embedding: -0,0183235...
info: WPFArcFaceApi.Controllers.ArcFaceController[0]
      User gets image info by id = 14 from database.
info: WPFArcFaceApi.Controllers.ArcFaceController[0]
      For image with id = 14 find embedding: -0,0471677...
info: WPFArcFaceApi.Controllers.ArcFaceController[0]
      User adds image with title 'face1.png' to database.
info: WPFArcFaceApi.Controllers.ArcFaceController[0]
      Image with title 'face1.png' is already in database with id = 14
info: WPFArcFaceApi.Controllers.ArcFaceController[0]
      User gets image info by id = 14 from database.
info: WPFArcFaceApi.Controllers.ArcFaceController[0]
      For image with id = 14 find embedding: -0,0471677...
info: WPFArcFaceApi.Controllers.ArcFaceController[0]
      User adds image with title 'face3.jpeg' to database.
info: WPFArcFaceApi.Controllers.ArcFaceController[0]
      Image with title 'face3.jpeg' was added to database (id = 15)
info: WPFArcFaceApi.Controllers.ArcFaceController[0]
      User gets image info by id = 15 from database.
info: WPFArcFaceApi.Controllers.ArcFaceController[0]
      For image with id = 15 find embedding: -0,0020563...
info: WPFArcFaceApi.Controllers.ArcFaceController[0]
      User deletes all images from database.
info: WPFArcFaceApi.Controllers.ArcFaceController[0]
      User gets all images ids from database.
info: WPFArcFaceApi.Controllers.ArcFaceController[0]
      GetImagesList result contains 0 images with IDs:
```
