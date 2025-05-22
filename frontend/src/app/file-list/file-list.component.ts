// file-list.component.ts
import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatTableModule } from '@angular/material/table';
import { RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { StorageService } from '../storage.service';
import { BlobDetailsDialogComponent } from "../blob-details/blob-details.component";
import { MatDialog } from "@angular/material/dialog";
import { saveAs } from 'file-saver';

@Component({
  selector: 'app-file-list',
  standalone: true,
  imports: [
    CommonModule,
    MatToolbarModule,
    MatButtonModule,
    MatTableModule,
    RouterModule,
    MatCardModule,
  ],
  templateUrl: './file-list.component.html',
  styleUrls: ['./file-list.component.scss'],
  providers: [StorageService],
})
export class FileListComponent implements OnInit {
  files: any[] = [];
  displayedColumns: string[] = ['name', 'size', 'lastModified', 'sasUrl', 'downloadByte', 'downloadStream'];

  constructor(private storageService: StorageService, private dialog: MatDialog) {}

  ngOnInit(): void {
    this.getFiles();
  }

  getFiles(): void {
    this.storageService
      .getFiles("my-container")
      .subscribe((data: any) => (this.files = data));
  }

  openFile(file: any): void {
    window.open(file.url, '_blank');
  }

  openDetails(file: any): void {
    this.dialog.open(BlobDetailsDialogComponent, {
      width: '1900px',
      height: '800px',
      data: file,
    });
  }

  downloadBytes(file: any): void {
    this.storageService.downloadBytes(file).subscribe({
      next: (blob) => {
        saveAs(blob, file.name);
      },
      error: (error) => {
        console.error('Error downloading file:', error);
      }
    });
  }

  downloadStream(file: any): void {
    this.storageService.downloadStream(file).subscribe({
      next: (blob) => {
        saveAs(blob, file.name);
      },
      error: (error) => {
        console.error('Error streaming file:', error);
      }
    });
  }
}
