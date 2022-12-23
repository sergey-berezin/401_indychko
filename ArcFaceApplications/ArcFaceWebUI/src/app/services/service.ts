import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { catchError, map, Observable } from "rxjs";
import { ImageFromDb } from "../models/models";


@Injectable()
export class Service {
	constructor(private http: HttpClient) {
	}

    serverAddress: string = 'api/arcFace/images'

  getImagesIdsFromDb(): Observable<number[]> {
    return this.http.get<number[]>(this.serverAddress).pipe(
            map(res => { return res }),
            catchError(err => { throw err })
        );
  }

  getImagesById(id: number): Observable<ImageFromDb> {
    return this.http.get<any>(this.serverAddress + "/id?id=" + id.toString()).pipe(
      map(res => {
        let img: ImageFromDb = {
          Title: res['title'],
          Id: res['id'],
          Embedding: res['embedding'],
          Image: res['image']
        }
        return img;
      }),
      catchError(err => { throw err })
    );
  }

  deleteAllFromDb() {
    return this.http.delete(this.serverAddress).pipe(
      catchError(err => { throw err })
    );
  }

  saveImageToDb(title: string, image: string) {
    let body = {
      "image": image.split(',')[1],
      "title": title
    }

    return this.http.post(this.serverAddress + '/add', body);
  }


  countSimilarity(first: number[], second: number[]): number {
    let res: number[] = [];
    for (let i = 0; i < first.length; i++) {
      res.push(first[i] * second[i]);
    }
    let sum = res.reduce((accumulator, current) => {
      return accumulator + current;
    }, 0);


    return Math.round((sum + Number.EPSILON) * 100) / 100;
  }

  countDistance(first: number[], second: number[]): number {
    let res: number[] = [];
    for (let i = 0; i < first.length; i++) {
      res.push(first[i] - second[i]);
    }

    return Math.round((this.length(res) + Number.EPSILON) * 100) / 100; 
  }

  length(array: number[]): number {
    return Math.sqrt(array.map(item => item * item).reduce((accumulator, current) => {
      return accumulator + current;
    }, 0));
  }
}
