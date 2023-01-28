import { Component, OnInit } from '@angular/core';
import { BsModalRef } from 'ngx-bootstrap/modal';

@Component({
  selector: 'app-roles-modal',
  templateUrl: './roles-modal.component.html',
  styleUrls: ['./roles-modal.component.css']
})
export class RolesModalComponent implements OnInit {
  // these properties is populated from openRolesModal method
  // inside UserManagementComponent class
  username = '';
  availableRoles: any[] = [];
  selectedRoles: any[] = [];

  constructor(public bsModalRef: BsModalRef) { }

  ngOnInit(): void {
  }

  // this method change the values of whether a checkbox is checked or not checked
  // based on which role the user is in
  updateChecked(checkedValue: string) {
    // if the index is equal to -1, that means it's not inside the selected roles array,
    // and if it's not inside the selected roles array and it's been checked,
    // then we want to add it to the selected roles array,
    // otherwise we want to remove it
    const index = this.selectedRoles.indexOf(checkedValue);
    index !== -1 ? this.selectedRoles.splice(index, 1) : this.selectedRoles.push(checkedValue);

  }

}
