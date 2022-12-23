export interface ImageFromDb {
	Id: number;
  Title: string;
  Image: string;
    Embedding: number[]
}

export interface ImageFromFolder {
    Path: string;
    Title: string;
    Image: string;
    Embedding?: number[]
}
