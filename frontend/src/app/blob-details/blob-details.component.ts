import { CommonModule } from "@angular/common";
import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { environment } from "../../environments/environment";

@Component({
  selector: 'app-blob-details',
  templateUrl: './blob-details.component.html',
  styleUrls: ['./blob-details.component.scss'],
  imports: [CommonModule]
})
export class BlobDetailsDialogComponent {

  environment = environment;
  constructor(
    public dialogRef: MatDialogRef<BlobDetailsDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any
  ) {}

  closeDialog(): void {
    this.dialogRef.close();
  }



}
