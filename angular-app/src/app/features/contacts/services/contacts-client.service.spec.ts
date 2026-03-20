import { TestBed } from '@angular/core/testing';

import { ContactsClientService } from './contacts-client.service';

describe('ContactsClientService', () => {
  let service: ContactsClientService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ContactsClientService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
