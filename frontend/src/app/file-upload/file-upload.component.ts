import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { StorageService } from '../storage.service';
import { RouterModule } from "@angular/router";

@Component({
  selector: 'app-file-upload',
  standalone: true,
  imports: [
    CommonModule,
    MatToolbarModule,
    MatButtonModule,
    MatListModule,
    MatIconModule,
    MatProgressBarModule,
      RouterModule,
  ],
  templateUrl: './file-upload.component.html',
  styleUrls: ['./file-upload.component.scss'],
  providers: [StorageService]
})
export class FileUploadComponent {
  files: File[] = [];
  uploading: boolean = false;
  progress: number = 0;

  constructor(private storageService: StorageService) {}

  onDrop(event: DragEvent): void {
    event.preventDefault();
    if (event.dataTransfer?.files) {
      Array.from(event.dataTransfer.files).forEach(file => this.files.push(file));
    }
  }

  onDragOver(event: DragEvent): void {
    event.preventDefault();
  }

  removeFile(index: number): void {
    this.files.splice(index, 1);
  }

  uploadFiles(): void {
    if (this.files.length === 0) return;

    this.uploading = true;
    this.progress = 0;

    this.files.forEach((file, index) => {
      const totalFiles = this.files.length;
      this.storageService.uploadFile(file).subscribe(() => {
        this.progress = ((index + 1) / totalFiles) * 100;
        if (index + 1 === totalFiles) {
          this.uploading = false;
          this.files = [];
        }
      });
    });
  }
}
