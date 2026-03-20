import { Component, EventEmitter, Output } from '@angular/core';
import { MatSliderModule } from '@angular/material/slider';

export type AgeFilter = {
  min: number;
  max: number;
};

@Component({
  selector: 'app-age-filter',
  imports: [MatSliderModule],
  templateUrl: './age-filter.component.html',
  styleUrl: './age-filter.component.scss',
})
export class AgeFilterComponent {
  @Output() filterRange = new EventEmitter<AgeFilter>();

  readonly minAge: number = 0;
  readonly maxAge: number = 100;
  filteredMin: number = 18;
  filteredMax: number = 60;

  onMinChanged(event: Event): void {
    const inputElement = event.target as HTMLInputElement;
    this.filteredMin = Number(inputElement.value);
    
    this.filterRange.emit({
      min: this.filteredMin,
      max: this.filteredMax,
    });
  }

  onMaxChanged(event: Event): void {
    const inputElement = event.target as HTMLInputElement;
    this.filteredMax = Number(inputElement.value);
    
    this.filterRange.emit({
      min: this.filteredMin,
      max: this.filteredMax,
    });
  }
}
