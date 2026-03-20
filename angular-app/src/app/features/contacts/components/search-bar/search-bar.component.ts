import { CommonModule } from '@angular/common';
import { Component, EventEmitter, input, Output } from '@angular/core';
import { FormControl, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import {  distinctUntilChanged } from 'rxjs';

@Component({
  selector: 'app-search-bar',
  imports: [
    FormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatAutocompleteModule,
    ReactiveFormsModule,
    CommonModule,
  ],
  templateUrl: './search-bar.component.html',
  styleUrl: './search-bar.component.scss',
})
export class SearchBarComponent {
  @Output() searchText = new EventEmitter<string>();
  filteredOptions = input<string[]>([]);
  searchControl = new FormControl('');

  constructor() {
    this.searchControl.valueChanges
      .pipe(
        distinctUntilChanged(), // Only emit if the value has changed
      )
      .subscribe((value) => {
        this.searchText.emit(value ?? undefined);
      });
  }
}
