import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';

import { Nav } from './nav/nav';
import { ErrorModal } from './shared/error-modal/error-modal';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, Nav, ErrorModal],
  templateUrl: './app.html',
  styleUrl: './app.css',
})
export class App {}
