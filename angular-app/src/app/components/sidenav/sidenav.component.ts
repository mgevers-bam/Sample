import { CommonModule } from '@angular/common';
import { Component, signal } from '@angular/core';
import { RouterModule } from '@angular/router';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';

export type NavbarItem = {
  icon: string;
  label: string;
  route?: string;
};

@Component({
  selector: 'app-sidenav',
  imports: [CommonModule, MatListModule, MatIconModule, RouterModule],
  templateUrl: './sidenav.component.html',
  styleUrl: './sidenav.component.scss',
})
export class SidenavComponent {
  navbarItems = signal<NavbarItem[]>([
    {
      icon: 'home',
      label: 'Home',
      route: 'home',
    },
    {
      icon: 'contact_phone',
      label: 'Contacts',
      route: 'contacts',
    },
    {
      icon: 'person',
      label: 'People',
      route: 'people',
    },
  ]);
}
