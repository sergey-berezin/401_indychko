# Лабораторная работа №1 
## Вариант 1

### Сборка Nuget пакета
1. Скачать ![нейронную сеть ArcFace](https://github.com/onnx/models/blob/main/vision/body_analysis/arcface/model/arcfaceresnet100-8.onnx).
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
