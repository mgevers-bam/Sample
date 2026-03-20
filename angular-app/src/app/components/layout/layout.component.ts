import { Component, computed, OnInit, Renderer2, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';

import { SidenavComponent } from '../sidenav/sidenav.component';

@Component({
  selector: 'app-layout',
  imports: [
    RouterOutlet,
    MatToolbarModule,
    MatButtonModule,
    MatIconModule,
    MatSidenavModule,
    SidenavComponent,
    MatSlideToggleModule,
  ],
  templateUrl: './layout.component.html',
  styleUrl: './layout.component.scss',
})
export class LayoutComponent {
  constructor() {}

  isDarkMode = signal(true);
  isCollapsed = signal(false);
  sidenavWidth = computed(() => (this.isCollapsed() ? '65px' : '250px'));

  ngOnInit(): void {
    const savedTheme = this.loadThemeName();

    this.isDarkMode.set(savedTheme === 'dark');
  }

  toggleTheme(isDarkMode: boolean): void {
    const theme = isDarkMode ? 'dark' : 'light';
    localStorage.setItem('theme', theme);

    this.isDarkMode.set(theme === 'dark');
    console.log('Theme changed to:', theme);
  }

  private loadThemeName(): 'dark' | 'light' {
    const storedTheme = localStorage.getItem('theme');

    if (storedTheme) {
      return storedTheme === 'dark' ? 'dark' : 'light';
    }

    return window.matchMedia('(prefers-color-scheme: dark)').matches
      ? 'dark'
      : 'light';
  }
}
