import { ChangeDetectorRef, Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { StorageService, UploadProgress } from '../storage.service';
import { RouterModule } from "@angular/router";
import { MatSnackBar, MatSnackBarModule } from "@angular/material/snack-bar";
import { Subject } from "rxjs";

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
    MatSnackBarModule,
      RouterModule,
  ],
  templateUrl: './file-upload.component.html',
  styleUrls: ['./file-upload.component.scss'],
  providers: [StorageService]
})
export class FileUploadComponent {

  isDragging = false;
  isUploading = false;
  errorMessage = '';
  currentUpload: {
    fileName: string;
    progress: UploadProgress;
  } | null = null;
  files: File[] = [];
  uploading: boolean = false;
  progress: number = 0; // Progress value (0-100)
  progress$ = new Subject<number>();

  constructor(private storageService: StorageService,
    private cdr: ChangeDetectorRef,
    private snackBar: MatSnackBar) {}

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

  UploadFilesChunk(): void {
    this.uploading = true;
    if (!this.files.length) {
      alert('Please select a file first!');
      return;
    }

    this.storageService.uploadChunkFile(this.files[0], 'my-container').subscribe({
      next: (event) => {
        if (event.progress !== undefined) {
          if (event.progress === 100) {
            setTimeout(() => {
              console.log('All files uploaded successfully.');
              this.showToast('All files uploaded successfully!', 'success');
              this.reset(); // Reset files and progress after upload
            }, 2000);
          }
        }
        console.log('Progress:', event);
        this.progress = event.progress;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Error uploading file:', err);
        this.uploading = false;
        this.cdr.detectChanges();
      },
      complete: () => {
        console.log('File upload completed!');
        this.reset();
      },
    });

  }


  UploadFileStream(): void {
    if (!this.files) {
      alert('Please select a file!');
      return;
    }
    this.uploading = true;
    const containerName = 'my-container'; // Replace with your container name
    const blobName = this.files[0].name;

    this.storageService.streamUpload(this.files[0], containerName, blobName).subscribe({
      next: () => {
        this.showToast('File uploaded successfully!', 'success');
        this.reset();
      },
      error: (error) => {
        console.error('Error during upload:', error);
      },
      complete: () => {
        this.uploading = false;
      }
    });
  }

  uploadMultipartFormData(){
    if (!this.files) {
      alert('Please select a file!');
      return;
    }
    const containerName = 'my-container'; // Replace with your container name
    const blobName = this.files[0].name;
    this.storageService.uploadFile(this.files[0], containerName, blobName).subscribe({
      next: (value) => {
        this.progress = value;
      },
      error: (error) => {
        console.error('Progress subscription error:', error);
      },
      complete: () => {
        console.log('File uploaded successfully!');
        this.showToast('File uploaded successfully!', 'success');
        this.reset();
      }
    });

  }

  UploadLargeFile() {
    if (!this.files || this.files.length === 0) {
      alert('Please select a file!');
      return;
    }
    
    // Don't proceed if already uploading
    if (this.uploading) {
      return;
    }
    
    this.uploading = true;
    this.errorMessage = '';
    const file = this.files[0];
    
    // Initialize progress tracking
    this.currentUpload = {
      fileName: file.name,
      progress: { loaded: 0, total: file.size, percentage: 0 }
    };
    
    // Single upload subscription
    this.storageService.uploadLargeFile(
      file,
      'my-container',
      (progress) => {
        // Update progress in UI
        this.currentUpload = {
          fileName: file.name,
          progress
        };
        this.progress = progress.percentage;
        this.cdr.detectChanges(); // Ensure UI updates
      }
    ).subscribe({
      next: (response) => {
        console.log('Upload completed successfully:', response);
        
        // Ensure the UI shows 100% before showing success
        this.progress = 100;
        this.currentUpload = {
          fileName: file.name,
          progress: { loaded: file.size, total: file.size, percentage: 100 }
        };
        
        // Show the success message and force change detection
        this.showToast('File uploaded successfully to Azure Blob Storage!', 'success');
        this.cdr.detectChanges();
        
        // Keep 100% progress visible for a moment before resetting
        setTimeout(() => {
          this.reset();
        }, 2000); // Increased timeout to ensure message is visible
      },
      error: (error) => {
        console.error('Upload failed:', error);
        this.errorMessage = `Upload failed: ${error.message}`;
        this.showToast(`Upload failed: ${error.message}`, 'error');
        this.uploading = false;
        this.currentUpload = null;
        this.cdr.detectChanges();
      },
      complete: () => {
        console.log('Upload observable completed');
        
        // Add a backup success message in complete handler in case 'next' isn't called
        if (this.uploading) {
          this.showToast('File upload completed successfully!', 'success');
          this.cdr.detectChanges();
          
          // Reset after a delay
          setTimeout(() => {
            this.reset();
          }, 2000);
        }
      }
    });
  }

  reset(): void {
    this.files = []; // Clear the files array
    this.progress = 0; // Reset progress
    this.uploading = false; // Reset uploading state
    this.cdr.detectChanges(); // Force Angular to update the view
  }

  showToast(message: string, type: 'success' | 'error' | 'info' | 'warn'): void {
    let panelClass = '';

    switch (type) {
      case 'success':
        panelClass = 'toast-success';
        break;
      case 'error':
        panelClass = 'toast-error';
        break;
      case 'info':
        panelClass = 'toast-info';
        break;
      case 'warn':
        panelClass = 'toast-warn';
        break;
    }

    this.snackBar.open(message, 'Close', {
      duration: 8000, // Increased duration to 8 seconds for better visibility
      horizontalPosition: 'center', // Changed to center for better visibility
      verticalPosition: 'top', // Position: top
      panelClass: [panelClass, 'toast-notification'], // Added a general class for easier customization
    });
  }
}
