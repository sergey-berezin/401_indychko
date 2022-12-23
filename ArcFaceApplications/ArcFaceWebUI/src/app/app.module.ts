import { HttpClientModule } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { MatDividerModule } from '@angular/material/divider';
import { MatListModule } from '@angular/material/list';
import { AppComponent } from './app.component';
import { Service } from './services/service';
import { MatIconModule } from '@angular/material/icon';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatCardModule } from '@angular/material/card';


@NgModule({
  declarations: [
    AppComponent
  ],
  imports: [
    BrowserModule, HttpClientModule, MatDividerModule, MatListModule, MatIconModule, MatCheckboxModule, MatProgressSpinnerModule, MatCardModule
  ],
  providers: [
    MatDividerModule, MatListModule, Service, MatIconModule, MatCheckboxModule, MatProgressSpinnerModule, MatCardModule
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
