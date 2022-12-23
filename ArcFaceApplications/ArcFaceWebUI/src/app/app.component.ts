import { HttpClient } from '@angular/common/http';
import { Component, ViewChild } from '@angular/core';
import { MatSelectionList } from '@angular/material/list';
import { DomSanitizer } from '@angular/platform-browser';
import { ImageFromDb, ImageFromFolder } from './models/models';
import { forkJoin } from 'rxjs';
import { Service } from './services/service';

@Component({
	selector: 'app-root',
	templateUrl: './app.component.html',
	styleUrls: ['./app.component.css']
})
export class AppComponent {
  @ViewChild("folderImgs") folderImgs!: MatSelectionList;

    imagesFromFolder: ImageFromFolder[] = [];

  imagesFromDb: ImageFromDb[] = [];

  loadingImagesFromDb: boolean = true;

  displayResults: boolean = false;

  loadingResults: boolean = false;

  constructor(private imageService: Service)
  {
    this.fillListWithImagesFromDb(true);
  }


  processSelectedImages(files: FileList | null) {
    if (files == null) return;

    for (let i = 0; i < files.length; i++) {

      const reader = new FileReader();

      reader.addEventListener("load", () => {
        // convert image file to base64 string
        let base64Img = reader.result as string;
        let newImage: ImageFromFolder = {
          Title: files[i].name,
          Path: files[i].webkitRelativePath,
          Image: base64Img
        };
        this.imagesFromFolder.push(newImage);
      }, false);

      reader.readAsDataURL(files[i]);
    }
  }


  async fillListWithImagesFromDb(init: boolean = false) {

    if (init) {
      await this.delay(1500)
    }

    this.loadingImagesFromDb = true;

    let list2 = document.getElementById("list2");

    this.imagesFromDb = [];

    document.getElementById("deleteAllButton")?.setAttribute("disabled", "disabled");

    if (list2 == null) return;

    list2.innerHTML = ''

    this.imageService.getImagesIdsFromDb().subscribe(result => {
    
      if (list2 == null) return;

      const list = document.createElement('ul')
      list.setAttribute("style", "list-style-type:none;")

      list2.appendChild(list);

      this.imagesFromFolder = [];

      if (result.length == 0) {
        this.loadingImagesFromDb = false;
        list2.innerHTML = "<br/><center>Database is empty</center>"
      }

      for (let i = 0; i < result.length; i++) {

        const listItem = document.createElement("li");

        const info = document.createElement("span");

        list.appendChild(listItem)

        this.imageService.getImagesById(result[i]).subscribe(res => {
          this.imagesFromDb.push(res);

          const img = document.createElement("img");
          img.src = `data:image/png;base64,${res.Image}`;
          img.height = 60;

          info.innerHTML = res.Title + "<br/>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;" +
            "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;ID: "
            + res.Id.toString();

          info.setAttribute("style", "position: relative; bottom: 30px; right:10px; left: 10px; line - height: 1px;")

          listItem.appendChild(img);

          listItem.setAttribute("style", "float: left; padding: 10px")

          listItem.appendChild(info);

          if (this.imagesFromDb.length > 0) {
            document.getElementById("deleteAllButton")?.removeAttribute("disabled");
            if (i == (result.length - 1)) {
              this.loadingImagesFromDb = false;
            }
          }
        });
      }
    });
  }

  saveImageToDatabase(files: FileList | null) {
    if (files == null) return;

    this.loadingImagesFromDb = true;

    this.imagesFromDb = [];

    let list2 = document.getElementById("list2");

    this.imagesFromDb = [];

    document.getElementById("deleteAllButton")?.setAttribute("disabled", "disabled");

    if (list2 == null) return;

    list2.innerHTML = ''

    for (let i = 0; i < files.length; i++) {

      const reader = new FileReader();

      reader.addEventListener("load", () => {
        // convert image file to base64 string
       let image = reader.result as string;
        this.imageService.saveImageToDb(files[i].name, image).subscribe(() => {
          if (i == files.length - 1) {
            this.fillListWithImagesFromDb();
          }
        });
      }, false);

      reader.readAsDataURL(files[i]);
    }
  }

  deleteAllFromDb() {

    this.loadingImagesFromDb = true;

    this.imagesFromDb = [];

    let list2 = document.getElementById("list2");

    this.imagesFromDb = [];

    document.getElementById("deleteAllButton")?.setAttribute("disabled", "disabled");

    if (list2 == null) return;

    list2.innerHTML = ''

    this.imageService.deleteAllFromDb().subscribe(() => {

      let list2 = document.getElementById("list2");

      document.getElementById("deleteAllButton")?.setAttribute("disabled", "disabled");

      if (list2 == null) return;

      list2.innerHTML = ''

      this.loadingImagesFromDb = false;
    });
  }

  analyseImages() {
    if (this.folderImgs.selectedOptions.selected.length == 0) {
      let errordiv = document.getElementById("errorDiv");
      if (errordiv == null) return;
      errordiv.innerHTML = "<br/><b>Select images first!</b><br/>"
      return;
    }

    this.loadingResults = true;

    const rowObjects = this.folderImgs.selectedOptions.selected.map(option => {
      let img = option.value as ImageFromFolder;
      return this.imageService.saveImageToDb(img.Title, img.Image);
    });

    forkJoin(rowObjects)
      .subscribe((result: Array<any>) => {
        let ids: number[] = result;
        const rowObjects = ids.map(id => this.imageService.getImagesById(id));
        forkJoin(rowObjects)
          .subscribe(async (result: Array<any>) => {
            this.displayResults = true;

            let imgs: ImageFromDb[] = result;
            imgs.forEach(img => img.Embedding = this.parseEmbedding(img.Embedding.toString()));

            await this.delay(100);

            const resultsDiv = document.getElementById("results") as HTMLElement;

            resultsDiv.innerHTML = '';

            const table = document.createElement("table");

            resultsDiv.appendChild(table);

            for (let i = -1; i < imgs.length; i++) {
              const row = document.createElement("tr");
              table.appendChild(row);

              for (let j = -1; j < imgs.length; j++) {
                if ((i == -1) && (j == -1)) {
                  const empty = document.createElement("td");
                  empty.textContent = "    "
                  row.appendChild(empty);
                } else if (i == -1) {
                  const empty = document.createElement("td");
                  row.appendChild(empty);
                  const imgJ = document.createElement("img");
                  imgJ.src = `data:image/png;base64,${imgs[j].Image}`;
                  imgJ.height = 90;
                  empty.appendChild(imgJ);
                }
                else if (j == -1) {
                  const empty = document.createElement("td");
                  row.appendChild(empty);
                  const imgJ = document.createElement("img");
                  imgJ.src = `data:image/png;base64,${imgs[i].Image}`;
                  imgJ.height = 90;
                  empty.appendChild(imgJ);
                } else {
                  const empty = document.createElement("td");
                  row.appendChild(empty);
                  const span = document.createElement("center");
                  span.innerHTML = `${this.imageService.countDistance(imgs[i].Embedding, imgs[j].Embedding)} / ${this.imageService.countSimilarity(imgs[i].Embedding, imgs[j].Embedding)}`
                  empty.appendChild(span)
                }
              }
            }
            this.loadingResults = false;
          });
      })
  }

    delay(ms: number) {
      return new Promise(resolve => setTimeout(resolve, ms));
  }

  parseEmbedding(emb: string): number[] {
    return emb.split(' ').map((token: string) => parseFloat(token.replace(',', '.')));
  }



}

