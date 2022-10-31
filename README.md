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
![WPF example](https://postimg.cc/0bbnLcMn)
