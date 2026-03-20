import { Routes } from '@angular/router';
import { HomePageComponent } from './pages/home-page/home-page.component';
import { ContactsPageComponent } from './pages/contacts-page/contacts-page.component';
import { PersonPageComponent } from './pages/person-page/person-page.component';

export const routes: Routes = [
    {
        path: '',
        pathMatch: 'full',
        redirectTo: 'home',
    },
    {
        path: 'home',
        component: HomePageComponent,
    },
    {
        path: 'contacts',
        component: ContactsPageComponent,
    },
    {
        path: 'people',
        component: PersonPageComponent
    }
];
