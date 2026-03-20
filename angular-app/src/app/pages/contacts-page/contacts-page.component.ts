import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDividerModule } from '@angular/material/divider';
import { BehaviorSubject, combineLatest, map, Observable } from 'rxjs';

import {
  Contact,
  ContactsClientService,
} from '../../features/contacts/services/contacts-client.service';
import { ContactCardComponent } from '../../features/contacts/components/contact-card/contact-card.component';
import { SearchBarComponent } from '../../features/contacts/components/search-bar/search-bar.component';
import {
  AgeFilter,
  AgeFilterComponent,
} from '../../features/contacts/components/age-filter/age-filter.component';

type ContactWrapper = {
  contact: Contact;
  isVisible: boolean;
};

@Component({
  selector: 'app-contacts-page',
  imports: [
    CommonModule,
    ContactCardComponent,
    SearchBarComponent,
    AgeFilterComponent,
    MatDividerModule,
  ],
  providers: [ContactsClientService],
  templateUrl: './contacts-page.component.html',
  styleUrl: './contacts-page.component.scss',
})
export class ContactsPageComponent {
  private readonly contactsClient: ContactsClientService = inject(
    ContactsClientService
  );
  private readonly searchTextSubject = new BehaviorSubject<string>('');
  private readonly ageFilterSubject = new BehaviorSubject<AgeFilter>({
    min: 0,
    max: 100,
  });

  readonly filteredContacts$: Observable<ContactWrapper[]> = combineLatest([
    this.contactsClient.getContacts(),
    this.searchTextSubject,
    this.ageFilterSubject,
  ]).pipe(
    map(([allContacts, searchText, ageFilter]) => {
      const filteredContacts = allContacts
        .filter((contact) => contact.name.toLowerCase().includes(searchText))
        .filter(
          (contact) =>
            contact.age >= ageFilter.min && contact.age <= ageFilter.max
        );

      const contactWrappers: ContactWrapper[] = allContacts.map((contact) => {
        return {
          contact,
          isVisible: filteredContacts.some(
            (filteredContact) => filteredContact.id === contact.id
          ),
        };
      });

      return contactWrappers;
    })
  );

  readonly filteredNames$: Observable<string[]> = this.filteredContacts$.pipe(
    map((contacts) => contacts.filter((c) => c.isVisible)),
    map((contacts) => {
      return contacts
        .map((c) => c.contact.name)
        .sort((a, b) => a.localeCompare(b))
        .slice(0, 5);
    })
  );

  onSearchTextChanged(value: string): void {
    this.searchTextSubject.next(value);
  }

  onAgeFilterChanged(value: AgeFilter): void {
    this.ageFilterSubject.next(value);
  }
}
