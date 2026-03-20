import { Component, input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatDividerModule } from '@angular/material/divider';

import { Contact } from '../../services/contacts-client.service';

@Component({
  selector: 'app-contact-card',
  imports: [ CommonModule, MatCardModule, MatDividerModule ],
  templateUrl: './contact-card.component.html',
  styleUrl: './contact-card.component.scss'
})
export class ContactCardComponent {
  contact = input<Contact | undefined>(undefined);
}
